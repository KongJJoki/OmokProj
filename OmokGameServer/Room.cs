using MemoryPack;
using PacketDefine;
using PacketTypes;
using SuperSocket.Common;

namespace OmokGameServer
{
    public class Room
    {
        public int RoomNumber { get; set; }
        public string NowTurnUser { get; set; }
        public string NextTurnUser { get; set; }
        public DateTime lastStonePlaceTime { get; set; }
        public DateTime gameStartTime { get; set; }
        public int ForceTurnChangeCount { get; set; }

        public int nowUserCount;
        int maxUserNumber;
        public bool isGameStart;

        public static Func<string, byte[], bool> sendFunc;

        List<User> userList = new List<User>();
        Dictionary<User, bool> readyStatusDictionary = new Dictionary<User, bool>();

        // 게임 관련들은 방말고 게임 안으로 넣기
        OmokBoard omokBoard = new OmokBoard();

        public void Init(int roomNum, int maxUserNum)
        {
            RoomNumber = roomNum;
            maxUserNumber = maxUserNum;
            nowUserCount = 0;
            isGameStart = false;
            ForceTurnChangeCount = 0;
        }


        public ERROR_CODE AddUser(User user)
        {
            if (nowUserCount >= maxUserNumber)
            {
                return ERROR_CODE.RoomEnterFailUserCountLimitExceed;
            }

            if (CheckUserExist(user))
            {
                return ERROR_CODE.RoomEnterFailAlreadyInRoom;
            }

            userList.Add(user);
            readyStatusDictionary[user] = false;
            nowUserCount++;

            return ERROR_CODE.None;
        }

        public ERROR_CODE RemoveUser(User user)
        {
            if (!CheckUserExist(user))
            {
                return ERROR_CODE.RoomLeaveFailNotInRoom;
            }

            userList.Remove(user);

            if (nowUserCount > 0)
            {
                nowUserCount--;
            }

            readyStatusDictionary.Remove(user);

            return ERROR_CODE.None;
        }

        public bool CheckUserExist(User user)
        {
            return userList.Contains(user);
        }
        public void Broadcast(string exceptSessionId, byte[] packet)
        {
            for (int i = 0; i < userList.Count; i++)
            {
                var user = userList[i];
                if (user.sessionId == exceptSessionId)
                {
                    continue;
                }
                sendFunc(user.sessionId, packet);
            }
        }
        public List<string> GetUserIds()
        {
            List<string> UserIds = new List<string>();

            for (int i = 0; i < nowUserCount; i++)
            {
                UserIds.Add(userList[i].userName);
            }

            return UserIds;
        }

        public bool GetReadyStatus(User user)
        {
            return readyStatusDictionary[user];
        }
        public void SetReadyStatus(User user, bool isReady)
        {
            readyStatusDictionary[user] = isReady;
        }
        public bool CheckAllReady()
        {
            foreach (var readyStatus in readyStatusDictionary.Values)
            {
                if (!readyStatus)
                {
                    return false;
                }
            }

            return true;
        }
        string FindOtherUserId(string userId)
        {
            foreach (var user in userList)
            {
                if (user.userName != userId)
                {
                    return user.userName;
                }
            }

            return null;
        }
        public User GetOtherUser(User myUser)
        {
            foreach(var user in userList)
            {
                if(user != myUser)
                {
                    return user;
                }
            }

            return null;
        }
        public OmokBoard GetOmokBoard()
        {
            return omokBoard;
        }
        public void SwapNowNextTurnUser()
        {
            string tmp = NowTurnUser;
            NowTurnUser = NextTurnUser;
            NextTurnUser = tmp;
        }

        public void GameFinish()
        {
            SetAllUserNotReady();
            isGameStart = false;
            omokBoard.ClearOmokBoard();
        }

        void SetAllUserNotReady()
        {
            foreach (var user in readyStatusDictionary.Keys)
            {
                readyStatusDictionary[user] = false;
            }
        }

        public void NotifyNewUserRoomEnter(User user)
        {
            if (nowUserCount == 0)
            {
                return;
            }

            PKTNTFRoomEnter roomEnterNTF = new PKTNTFRoomEnter();
            roomEnterNTF.UserId = user.userName;

            var bodyData = MemoryPackSerializer.Serialize(roomEnterNTF);
            var sendData = PacketToBytes.MakeBytes(PACKET_ID.RoomEnterNotify, bodyData);

            Broadcast(user.sessionId, sendData);
        }
        public void NotifyRoomMemberList(string sessionId)
        {
            if (nowUserCount == 0)
            {
                return;
            }

            PKTNTFRoomMember roomMemberNTF = new PKTNTFRoomMember();
            roomMemberNTF.UserIdList = GetUserIds();

            var bodyData = MemoryPackSerializer.Serialize(roomMemberNTF);
            var sendData = PacketToBytes.MakeBytes(PACKET_ID.RoomMemberNotify, bodyData);

            sendFunc(sessionId, sendData);
        }

        public void NotifyRoomLeave(User user)
        {
            if (nowUserCount == 0)
            {
                return;
            }

            PKTNTFRoomLeave roomLeaveNTF = new PKTNTFRoomLeave();
            roomLeaveNTF.UserId = user.userName;

            var bodyData = MemoryPackSerializer.Serialize(roomLeaveNTF);
            var sendData = PacketToBytes.MakeBytes(PACKET_ID.RoomLeaveNotify, bodyData);

            Broadcast("", sendData);
        }

        public void NotifyRoomChat(string userId, string message)
        {
            PKTNTFRoomChat roomChatNTF = new PKTNTFRoomChat();
            roomChatNTF.UserId = userId;
            roomChatNTF.Message = message;

            var bodyData = MemoryPackSerializer.Serialize(roomChatNTF);
            var sendData = PacketToBytes.MakeBytes(PACKET_ID.RoomChatNotify, bodyData);

            Broadcast("", sendData);
        }

        public void NotifyGameStart(string startUserId)
        {
            lastStonePlaceTime = DateTime.Now;
            gameStartTime = DateTime.Now;
            isGameStart = true;
            omokBoard.BlackUserId = startUserId;
            omokBoard.WhiteUserId = FindOtherUserId(startUserId);
            NowTurnUser = startUserId;
            NextTurnUser = omokBoard.WhiteUserId;

            PKTNTFGameStart gameStartNTF = new PKTNTFGameStart();
            gameStartNTF.StartUserId = startUserId;

            var bodyData = MemoryPackSerializer.Serialize(gameStartNTF);
            var sendData = PacketToBytes.MakeBytes(PACKET_ID.GameStartNotify, bodyData);

            Broadcast("", sendData);
        }

        public void NotifyOmokStonePlace(int posX, int posY)
        {
            lastStonePlaceTime = DateTime.Now;

            PKTNTFOmokStonePlace omokStonePlaceNTF = new PKTNTFOmokStonePlace();

            omokStonePlaceNTF.NextTurnUserId = NextTurnUser;

            omokStonePlaceNTF.PosX = posX;
            omokStonePlaceNTF.PosY = posY;

            var bodyData = MemoryPackSerializer.Serialize(omokStonePlaceNTF);
            var sendData = PacketToBytes.MakeBytes(PACKET_ID.OmokStonePlaceNotify, bodyData);

            Broadcast("", sendData);

            SwapNowNextTurnUser();
        }

        public void NotifyOmokWin(User winUser)
        {
            GameFinish();

            PKTNTFOmokWin omokWinNTF = new PKTNTFOmokWin();
            omokWinNTF.WinUserId = winUser.userName;

            var bodyData = MemoryPackSerializer.Serialize(omokWinNTF);
            var sendData = PacketToBytes.MakeBytes(PACKET_ID.OmokWinNotify, bodyData);

            sendFunc(winUser.sessionId, sendData);
        }

        public void NotifyOmokLose(User loseUser)
        {
            GameFinish();

            PKTNTFOmokLose omokLoseNTF = new PKTNTFOmokLose();
            omokLoseNTF.LoseUserId = loseUser.userName;

            var bodyData = MemoryPackSerializer.Serialize(omokLoseNTF);
            var sendData = PacketToBytes.MakeBytes(PACKET_ID.OmokLoseNotify, bodyData);

            sendFunc(loseUser.sessionId, sendData);
        }

        public void NotifyGameForceFinish()
        {
            GameFinish();

            PKTNTFForceGameFinish forceGameFinish = new PKTNTFForceGameFinish();

            var bodyData = MemoryPackSerializer.Serialize(forceGameFinish);
            var sendData = PacketToBytes.MakeBytes(PACKET_ID.OmokForceFinish, bodyData);

            Broadcast("", sendData);
        }

        public void NotifyTimeOutTurnChange()
        {
            lastStonePlaceTime = DateTime.Now;
            ForceTurnChangeCount++;
            PKTNTFTurnChange turnChangeNTF = new PKTNTFTurnChange();
            turnChangeNTF.TurnGetUserId = NextTurnUser;

            SwapNowNextTurnUser();

            var bodyData = MemoryPackSerializer.Serialize(turnChangeNTF);
            var sendData = PacketToBytes.MakeBytes(PACKET_ID.TurnChangeNotify, bodyData);

            Broadcast("", sendData);
        }
    }

    public class GameWinEventArgs : EventArgs
    {
        public string WinUserId { get; }
        public string LoseUserId { get; }
        public GameWinEventArgs(string winUserId, string loseUserId)
        {
            WinUserId = winUserId;
            LoseUserId = loseUserId;
        }
    }
}