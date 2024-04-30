using MemoryPack;
using PacketDefine;
using PacketTypes;

namespace OmokGameServer
{
    public class Room
    {
        public int RoomNumber { get; set; }

        public int nowUserCount;
        int MaxUserNumber;
        public bool isGameStart;

        public static Func<string, byte[], bool> sendFunc;

        List<User> userList = new List<User>();
        Dictionary<User, bool> readyStatusDictionary = new Dictionary<User, bool>();

        public void Init(int roomNum, int maxUserNum)
        {
            RoomNumber = roomNum;
            MaxUserNumber = maxUserNum;
            nowUserCount = 0;
        }

        public ERROR_CODE AddUser(User user)
        {
            if (nowUserCount >= MaxUserNumber)
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
                if(user.sessionId == exceptSessionId)
                {
                    continue;
                }
                sendFunc(user.sessionId, packet);
            }
        }
        public List<string> GetUserIds()
        {
            List<string> UserIds = new List<string>();

            for(int i = 0; i < nowUserCount; i++)
            {
                UserIds.Add(userList[i].userId);
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
            foreach(var readyStatus in readyStatusDictionary.Values)
            {
                if(!readyStatus)
                {
                    return false;
                }
            }

            return true;
        }

        public void NotifyNewUserRoomEnter(User user)
        {
            if (nowUserCount == 0)
            {
                return;
            }

            PKTNTFRoomEnter roomEnterNTF = new PKTNTFRoomEnter();
            roomEnterNTF.UserId = user.userId;

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
            roomLeaveNTF.UserId = user.userId;

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

        public void NotifyGameStart()
        {

        }
    }
}