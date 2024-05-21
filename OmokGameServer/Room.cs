using GameServerClientShare;
using MemoryPack;
using PacketDefine;
using InPacketTypes;
using SuperSocket.Common;

namespace OmokGameServer
{
    public class Room
    {
        public int RoomNumber { get; set; }
        public int NowTurnUserUid { get; set; }
        public int NextTurnUserUid { get; set; }
        public DateTime lastStonePlaceTime { get; set; }
        public DateTime gameStartTime { get; set; }
        public int ForceTurnChangeCount { get; set; }

        public int nowUserCount;
        int maxUserNumber;
        public bool isGameStart;

        public static Func<string, byte[], bool> sendFunc;
        Action<ServerPacketData> pushPacketInProcessorFunc;
        Func<string, bool> userConnectionCloseFunc;

        List<User> userList = new List<User>();
        Dictionary<User, bool> readyStatusDictionary = new Dictionary<User, bool>();

        // 게임 관련들은 방말고 게임 안으로 넣기
        OmokBoard omokBoard = new OmokBoard();

        public void Init(int roomNum, int maxUserNum, Action<ServerPacketData> pushPacketInProcessorFunc, Func<string, bool> userConnectionCloseFunc)
        {
            RoomNumber = roomNum;
            maxUserNumber = maxUserNum;
            nowUserCount = 0;
            isGameStart = false;
            ForceTurnChangeCount = 0;
            this.pushPacketInProcessorFunc = pushPacketInProcessorFunc;
            this.userConnectionCloseFunc = userConnectionCloseFunc;
        }


        public SockErrorCode AddUser(User user)
        {
            if (nowUserCount >= maxUserNumber)
            {
                return SockErrorCode.RoomEnterFailUserCountLimitExceed;
            }

            if (CheckUserExist(user))
            {
                return SockErrorCode.RoomEnterFailAlreadyInRoom;
            }

            userList.Add(user);
            readyStatusDictionary[user] = false;
            nowUserCount++;

            return SockErrorCode.None;
        }

        public SockErrorCode RemoveUser(User user)
        {
            if (!CheckUserExist(user))
            {
                return SockErrorCode.RoomLeaveFailNotInRoom;
            }

            userList.Remove(user);

            if (nowUserCount > 0)
            {
                nowUserCount--;
            }

            readyStatusDictionary.Remove(user);

            return SockErrorCode.None;
        }

        void RemoveAllUser()
        {
            userList.Clear();
            readyStatusDictionary.Clear();
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
        public List<int> GetUserIds()
        {
            List<int> UserIds = new List<int>();

            for (int i = 0; i < nowUserCount; i++)
            {
                UserIds.Add(userList[i].uid);
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
        int FindOtherUserId(int uid)
        {
            foreach (var user in userList)
            {
                if (user.uid != uid)
                {
                    return user.uid;
                }
            }

            return 0;
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
            int tmp = NowTurnUserUid;
            NowTurnUserUid = NextTurnUserUid;
            NextTurnUserUid = tmp;
        }

        public void GameFinish()
        {
            SetAllUserNotReady();
            isGameStart = false;
            omokBoard.ClearOmokBoard();
            RemoveAllUser();
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
            roomEnterNTF.Uid = user.uid;

            var bodyData = MemoryPackSerializer.Serialize(roomEnterNTF);
            var sendData = PacketToBytes.MakeToPacket(PacketID.RoomEnterNotify, bodyData);

            Broadcast(user.sessionId, sendData);
        }
        public void NotifyRoomMemberList(string sessionId)
        {
            if (nowUserCount == 0)
            {
                return;
            }

            PKTNTFRoomMember roomMemberNTF = new PKTNTFRoomMember();
            roomMemberNTF.UidList = GetUserIds();

            var bodyData = MemoryPackSerializer.Serialize(roomMemberNTF);
            var sendData = PacketToBytes.MakeToPacket(PacketID.RoomMemberNotify, bodyData);

            sendFunc(sessionId, sendData);
        }

        public void NotifyRoomLeave(User user)
        {
            if (nowUserCount == 0)
            {
                return;
            }

            PKTNTFRoomLeave roomLeaveNTF = new PKTNTFRoomLeave();
            roomLeaveNTF.Uid = user.uid;

            var bodyData = MemoryPackSerializer.Serialize(roomLeaveNTF);
            var sendData = PacketToBytes.MakeToPacket(PacketID.RoomLeaveNotify, bodyData);

            Broadcast("", sendData);
        }

        public void NotifyRoomChat(int uid, string message)
        {
            PKTNTFRoomChat roomChatNTF = new PKTNTFRoomChat();
            roomChatNTF.Uid = uid;
            roomChatNTF.Message = message;

            var bodyData = MemoryPackSerializer.Serialize(roomChatNTF);
            var sendData = PacketToBytes.MakeToPacket(PacketID.RoomChatNotify, bodyData);

            Broadcast("", sendData);
        }

        public void NotifyGameStart(int startUserId)
        {
            lastStonePlaceTime = DateTime.Now;
            gameStartTime = DateTime.Now;
            isGameStart = true;

            omokBoard.Init(startUserId, FindOtherUserId(startUserId), pushPacketInProcessorFunc);

            NowTurnUserUid = startUserId;
            NextTurnUserUid = omokBoard.WhiteUserUid;

            PKTNTFGameStart gameStartNTF = new PKTNTFGameStart();
            gameStartNTF.StartUserUid = startUserId;

            var bodyData = MemoryPackSerializer.Serialize(gameStartNTF);
            var sendData = PacketToBytes.MakeToPacket(PacketID.GameStartNotify, bodyData);

            Broadcast("", sendData);
        }

        public void NotifyOmokStonePlace(int posX, int posY)
        {
            lastStonePlaceTime = DateTime.Now;

            PKTNTFOmokStonePlace omokStonePlaceNTF = new PKTNTFOmokStonePlace();

            omokStonePlaceNTF.NextTurnUserUid = NextTurnUserUid;

            omokStonePlaceNTF.PosX = posX;
            omokStonePlaceNTF.PosY = posY;

            var bodyData = MemoryPackSerializer.Serialize(omokStonePlaceNTF);
            var sendData = PacketToBytes.MakeToPacket(PacketID.OmokStonePlaceNotify, bodyData);

            Broadcast("", sendData);

            SwapNowNextTurnUser();
        }

        public void NotifyOmokWin(User winUser)
        {
            PKTNTFOmokWin omokWinNTF = new PKTNTFOmokWin();
            omokWinNTF.WinUserUid = winUser.uid;

            var bodyData = MemoryPackSerializer.Serialize(omokWinNTF);
            var sendData = PacketToBytes.MakeToPacket(PacketID.OmokWinNotify, bodyData);

            sendFunc(winUser.sessionId, sendData);
        }

        public void NotifyOmokLose(User loseUser)
        {
            PKTNTFOmokLose omokLoseNTF = new PKTNTFOmokLose();
            omokLoseNTF.LoseUserUid = loseUser.uid;

            var bodyData = MemoryPackSerializer.Serialize(omokLoseNTF);
            var sendData = PacketToBytes.MakeToPacket(PacketID.OmokLoseNotify, bodyData);

            sendFunc(loseUser.sessionId, sendData);
        }

        public void SaveGameResult(User winUser, User loseUser)
        {
            omokBoard.GameResultSave(winUser.uid, loseUser.uid);
        }

        public void NotifyGameForceFinish()
        {
            PKTNTFForceGameFinish forceGameFinish = new PKTNTFForceGameFinish();

            var bodyData = MemoryPackSerializer.Serialize(forceGameFinish);
            var sendData = PacketToBytes.MakeToPacket(PacketID.OmokForceFinish, bodyData);

            Broadcast("", sendData);
        }

        public void NotifyTimeOutTurnChange()
        {
            lastStonePlaceTime = DateTime.Now;
            ForceTurnChangeCount++;
            PKTNTFTurnChange turnChangeNTF = new PKTNTFTurnChange();
            turnChangeNTF.TurnGetUserUid = NextTurnUserUid;

            SwapNowNextTurnUser();

            var bodyData = MemoryPackSerializer.Serialize(turnChangeNTF);
            var sendData = PacketToBytes.MakeToPacket(PacketID.TurnChangeNotify, bodyData);

            Broadcast("", sendData);
        }
    }
}