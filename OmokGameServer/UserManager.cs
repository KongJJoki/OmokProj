using PacketDefine;
using SuperSocket.SocketBase.Logging;

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
            packet.SetPacketNoBody("", (Int16)PACKET_ID.InNTFCheckHeartBeat);

            pushPacketInProcessorFunc(packet);
        }
        
        public ERROR_CODE AddUserConnection(string sessionId)
        {
            User newUser = new User();
            newUser.SetUser(sessionId);
            
            bool isAddSuccess = AddUserToHeartBeatArray(newUser);

            if(!isAddSuccess)
            {
                return ERROR_CODE.ConnectFailUserCountLimitExceed;
            }
            else
            {
                sessionIduserDictionary[sessionId] = newUser;
                userSessionIdDictionary[newUser] = sessionId;

                return ERROR_CODE.None;
            }

        }

        public ERROR_CODE AddUserLogin(string sessionId, Int32 uid)
        {
            if (nowLoginUserList.Count > maxLoginUserCount)
            {
                return ERROR_CODE.LoginFailUserCountLimitExceed;
            }
            else
            {
                User newUser = sessionIduserDictionary[sessionId];

                nowLoginUserList.Add(newUser);
                SetUserName(sessionId, uid);

                return ERROR_CODE.None;
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

        public ERROR_CODE RemoveUser(string sessionId)
        {
            User user = GetUser(sessionId);

            bool isRemoveSuccess = sessionIduserDictionary.Remove(sessionId);

            if(!isRemoveSuccess)
            {
                return ERROR_CODE.RemoveFailNotExistSession;
            }

            userSessionIdDictionary.Remove(user);

            nowLoginUserList.Remove(user);

            return ERROR_CODE.None;
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

        public ERROR_CODE CheckUserConnected(string sessionId)
        {
            User user = sessionIduserDictionary[sessionId];

            if(user == null)
            {
                return ERROR_CODE.DisconnectFailUserNotExist;
            }
            else
            {
                return ERROR_CODE.None;
            }
        }

        public ERROR_CODE CheckUserLoginExist(string sessionId)
        {
            User user = sessionIduserDictionary[sessionId];

            if(user == null)
            {
                return ERROR_CODE.LoginFailNotConnected;
            }

            bool isUserExist = nowLoginUserList.Contains(user);
            
            if(isUserExist)
            {
                return ERROR_CODE.LoginFailAlreadyExistUser;
            }

            return ERROR_CODE.None;
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

        public bool CheckHeartBeatMeaningful(int userIndex)
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