using PacketDefine;
using SuperSocket.SocketBase.Logging;
using SockInternalPacket;
using GameServerClientShare;

namespace OmokGameServer
{
    public class UserManager
    {
        ServerOption serverOption;
        ILog mainLogger;
        Action<ServerPacketData> pushPacketInProcessorFunc;
        Func<string, bool> closeConnectionFunc;

        TimeSpan heartBeatLimitTime;
        Timer heartBeatCheckTimer;
        int maxLoginUserCount;
        
        Dictionary<string, User> sessionIduserDictionary = new Dictionary<string, User>();
        Dictionary<User, string> userSessionIdDictionary = new Dictionary<User, string>();
        List<User> nowLoginUserList = new List<User>();

        User[] userArrayForHeartBeatCheck;

        public void Init(ServerOption serverOption, ILog mainLogger, Action<ServerPacketData> pushPacketInProcessorFunc, Func<string, bool> closeConnectionFunc)
        {
            this.serverOption = serverOption;
            this.mainLogger = mainLogger;
            this.pushPacketInProcessorFunc = pushPacketInProcessorFunc;
            this.closeConnectionFunc = closeConnectionFunc;
            heartBeatLimitTime = new TimeSpan(0, 0, 0, serverOption.HeartBeatTimeLimitSecond);
            maxLoginUserCount = serverOption.RoomMaxCount * serverOption.RoomMaxUserCount;
            SetTimer();
            userArrayForHeartBeatCheck = new User[serverOption.MaxConnectionNumber];
        }

        public void SetTimer()
        {
            heartBeatCheckTimer = new Timer(SendHeartBeatCheckPacket, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(1000 / serverOption.TotalDivideNumber));
        }

        void SendHeartBeatCheckPacket(object state)
        {
            ServerPacketData packet = new ServerPacketData();
            packet.SetPacketNoBody("", (Int16)InPACKET_ID.InNTFCheckHeartBeat);

            pushPacketInProcessorFunc(packet);
        }
        
        public SockErrorCode AddUserConnection(string sessionId)
        {
            User newUser = new User();
            newUser.SetUser(sessionId);
            
            bool isAddSuccess = AddUserToHeartBeatArray(newUser);

            if(!isAddSuccess)
            {
                return SockErrorCode.ConnectFailUserCountLimitExceed;
            }
            else
            {
                sessionIduserDictionary[sessionId] = newUser;
                userSessionIdDictionary[newUser] = sessionId;

                return SockErrorCode.None;
            }

        }

        public SockErrorCode AddUserLogin(string sessionId, Int32 uid)
        {
            if (nowLoginUserList.Count > maxLoginUserCount)
            {
                return SockErrorCode.LoginFailUserCountLimitExceed;
            }
            else
            {
                User newUser = sessionIduserDictionary[sessionId];

                nowLoginUserList.Add(newUser);
                SetUserName(sessionId, uid);

                return SockErrorCode.None;
            }
        }

        public bool AddUserToHeartBeatArray(User user)
        {
            for(int i = 0; i < userArrayForHeartBeatCheck.Length; i++)
            {
                if (userArrayForHeartBeatCheck[i]==null)
                {
                    userArrayForHeartBeatCheck[i] = user;
                    user.myUserArrayIndex = i;
                    return true;
                }
            }

            return false;
        }

        public void SetUserName(string sessionId, Int32 uid)
        {
            User user = sessionIduserDictionary[sessionId];
            user.SetUserName(uid);
        }

        public SockErrorCode RemoveUser(string sessionId)
        {
            User user = GetUser(sessionId);

            bool isRemoveSuccess = sessionIduserDictionary.Remove(sessionId);

            if(!isRemoveSuccess)
            {
                return SockErrorCode.RemoveFailNotExistSession;
            }

            userSessionIdDictionary.Remove(user);

            nowLoginUserList.Remove(user);

            return SockErrorCode.None;
        }

        public bool RemoveUserFromArray(int userIndex)
        {
            User user = userArrayForHeartBeatCheck[userIndex];

            if(user == null)
            {
                return false;
            }

            if (userArrayForHeartBeatCheck[user.myUserArrayIndex] == null)
            {
                return false;
            }
            else
            {
                userArrayForHeartBeatCheck[user.myUserArrayIndex] = null;
                return true;
            }
        }

        public SockErrorCode CheckUserConnected(string sessionId)
        {
            User user = sessionIduserDictionary[sessionId];

            if(user == null)
            {
                return SockErrorCode.DisconnectFailUserNotExist;
            }
            else
            {
                return SockErrorCode.None;
            }
        }

        public SockErrorCode CheckUserLoginExist(string sessionId)
        {
            User user = sessionIduserDictionary[sessionId];

            if(user == null)
            {
                return SockErrorCode.LoginFailNotConnected;
            }

            bool isUserExist = nowLoginUserList.Contains(user);
            
            if(isUserExist)
            {
                return SockErrorCode.LoginFailAlreadyExistUser;
            }

            return SockErrorCode.None;
        }

        public bool CheckUserExistInHeartBeatArray(int index)
        {
            if (userArrayForHeartBeatCheck[index]==null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public User GetUser(string sessionId)
        {
            return sessionIduserDictionary[sessionId];
        }

        public string GetUserSessionIdByheartBeatIndex(int index)
        {
            User user = userArrayForHeartBeatCheck[index];

            return user.sessionId;
        }

        public bool CheckUserArrayIsNull(int index)
        {
            if (userArrayForHeartBeatCheck[index] == null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public TimeSpan CheckHeartBeatTimeDiff(int userIndex)
        {
            return DateTime.Now - userArrayForHeartBeatCheck[userIndex].lastHeartBeatTime;
        }

        public bool CheckAndCutHeartBeat(int userIndex)
        {
            if(CheckUserArrayIsNull(userIndex))
            {
                return false;
            }

            TimeSpan timeDiff = CheckHeartBeatTimeDiff(userIndex);
            if (timeDiff > heartBeatLimitTime)
            {
                string sessionId = GetUserSessionIdByheartBeatIndex(userIndex);

                closeConnectionFunc(sessionId);
                RemoveUserFromArray(userIndex);
                mainLogger.Debug($"force Disconnected");

                return false;
            }

            return true;
        }
    }
}