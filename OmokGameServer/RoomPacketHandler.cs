using Antlr4.Runtime.Tree;
using MemoryPack;
using PacketDefine;
using PacketTypes;

namespace OmokGameServer
{
    public class RoomPacketHandler : PacketHandler
    {
        List<Room> roomList;

        public void SetRoomList(List<Room> roomList)
        {
            this.roomList = roomList;
        }

        public void SetPacketHandler(Dictionary<int, Action<ServerPacketData>> packetHandlerDictionary)
        {
            packetHandlerDictionary.Add((int)PACKET_ID.ROOM_ENTER_REQUEST, RoomEnterRequest);
            packetHandlerDictionary.Add((int)PACKET_ID.ROOM_LEAVE_REQUEST, RoomLeaveRequest);
            packetHandlerDictionary.Add((int)PACKET_ID.ROOM_CHAT_REQUEST, RoomChatRequest);
        }
        public Room GetRoom(int roomNum)
        {
            int roomIndex = roomNum - MainServer.serverOption.RoomStartNumber;

            if (roomIndex < MainServer.serverOption.RoomStartNumber - 1 || roomIndex > MainServer.serverOption.RoomMaxCount)
            {
                return null;
            }

            return roomList[roomIndex];
        }
        public void RoomEnterRequest(ServerPacketData packet)
        {
            string sessionId = packet.SessionId;
            MainServer.MainLogger.Info($"SessionId({sessionId}) Request Room Enter");

            try
            {
                User user = userManager.GetUser(sessionId);

                if(user == null)
                {
                    RoomEnterRespond(ERROR_CODE.Room_Enter_Fail_Invalid_User, sessionId);
                    return;
                }

                if(user.isInRoom)
                {
                    RoomEnterRespond(ERROR_CODE.Room_Enter_Fail_Already_In_Room, sessionId);
                    return;
                }

                var requestData = MemoryPackSerializer.Deserialize<PKTReqRoomEnter>(packet.BodyData);

                Room room = GetRoom(requestData.RoomNumber);

                if(room == null)
                {
                    RoomEnterRespond(ERROR_CODE.Room_Enter_Fail_Not_Exist_Room, sessionId);
                    return;
                }

                ERROR_CODE errorCode = room.AddUser(user);
                if(errorCode != ERROR_CODE.None)
                {
                    RoomEnterRespond(errorCode, sessionId);
                    return;
                }

                user.SetRoomEnter(requestData.RoomNumber);

                RoomEnterRespond(errorCode, sessionId);

                NotifyRoomMember(sessionId, room);
                NotifyRoomEnter(user, room);

                MainServer.MainLogger.Info($"sessionId({sessionId}) enter {requestData.RoomNumber} Room");
            }
            catch(Exception ex)
            {
                MainServer.MainLogger.Error(ex.ToString());
            }
        }

        void RoomEnterRespond(ERROR_CODE errorCode, string sessionId)
        {
            PKTResRoomEnter roomEnterRes = new PKTResRoomEnter();
            roomEnterRes.Result = (int)errorCode;

            var bodyData = MemoryPackSerializer.Serialize(roomEnterRes);
            var sendData = PacketToBytes.MakeBytes(PACKET_ID.ROOM_ENTER_RESPOND, bodyData);

            sendFunc(sessionId, sendData);
        }
        void NotifyRoomEnter(User user, Room room)
        {
            if(room.nowUserCount == 0)
            {
                return;
            }

            PKTNTFRoomEnter roomEnterNTF = new PKTNTFRoomEnter();
            roomEnterNTF.UserId = user.userId;

            var bodyData = MemoryPackSerializer.Serialize(roomEnterNTF);
            var sendData = PacketToBytes.MakeBytes(PACKET_ID.ROOM_ENTER_NOTIFY, bodyData);

            room.Broadcast(user.sessionId, sendData);
        }
        void NotifyRoomMember(string sessionId, Room room)
        {
            if(room.nowUserCount == 0)
            {
                return;
            }

            PKTNTFRoomMember roomMemberNTF = new PKTNTFRoomMember();
            roomMemberNTF.UserIdList = room.GetUserIds();

            var bodyData = MemoryPackSerializer.Serialize(roomMemberNTF);
            var sendData = PacketToBytes.MakeBytes(PACKET_ID.ROOM_MEMBER_NOTIFY, bodyData);

            sendFunc(sessionId, sendData);
        }

        public void RoomLeaveRequest(ServerPacketData packet)
        {
            string sessionId = packet.SessionId;
            MainServer.MainLogger.Info($"SessionId({sessionId}) Request Room Leave");

            try
            {
                User user = userManager.GetUser(sessionId);

                if (user == null)
                {
                    RoomLeaveRespond(ERROR_CODE.Room_Leave_Fail_Invalid_User, sessionId);
                    return;
                }

                if(!user.isInRoom)
                {
                    RoomLeaveRespond(ERROR_CODE.Room_Leave_Fail_Not_In_Room, sessionId);
                    return;
                }

                var requestData = MemoryPackSerializer.Deserialize<PKTReqRoomEnter>(packet.BodyData);

                Room room = GetRoom(user.roomNumber);

                if(requestData.RoomNumber != user.roomNumber)
                {
                    RoomLeaveRespond(ERROR_CODE.Room_Leave_Fail_Not_In_Room, sessionId);
                    return;
                }

                ERROR_CODE errorCode = room.RemoveUser(user);
                if(errorCode!=ERROR_CODE.None)
                {
                    RoomLeaveRespond(errorCode, sessionId);
                    return;
                }

                user.SetRoomLeave();

                NotifyRoomLeave(user, room);
                RoomLeaveRespond(errorCode, sessionId);

                MainServer.MainLogger.Info($"sessionId({sessionId}) leave Room");
            }
            catch(Exception ex)
            {
                MainServer.MainLogger.Error(ex.ToString());
            }
        }

        void RoomLeaveRespond(ERROR_CODE errorCode, string sessionId)
        {
            PKTResRoomLeave roomLeaveRes = new PKTResRoomLeave();
            roomLeaveRes.Result = (int)errorCode;

            var bodyData = MemoryPackSerializer.Serialize(roomLeaveRes);
            var sendData = PacketToBytes.MakeBytes(PACKET_ID.ROOM_LEAVE_RESPOND, bodyData);

            sendFunc(sessionId, sendData);
        }
        void NotifyRoomLeave(User user, Room room)
        {
            if (room.nowUserCount == 0)
            {
                return;
            }

            PKTNTFRoomLeave roomLeaveNTF = new PKTNTFRoomLeave();
            roomLeaveNTF.UserId = user.userId;

            var bodyData = MemoryPackSerializer.Serialize(roomLeaveNTF);
            var sendData = PacketToBytes.MakeBytes(PACKET_ID.ROOM_LEAVE_NOTIFY, bodyData);

            room.Broadcast("", sendData);
        }

        public void RoomChatRequest(ServerPacketData packet)
        {
            string sessionId = packet.SessionId;
            MainServer.MainLogger.Info($"sessionId({sessionId} send Chat)");

            try
            {
                User user = userManager.GetUser(sessionId);
                if(user == null)
                {
                    RoomChatRespond(ERROR_CODE.Room_Chat_Fail_Invalid_User, sessionId);
                    return;
                }

                if(!user.isInRoom)
                {
                    RoomChatRespond(ERROR_CODE.Room_Chat_Fail_Not_In_Room, sessionId);
                    return;
                }

                RoomChatRespond(ERROR_CODE.None, sessionId);

                var requestData = MemoryPackSerializer.Deserialize<PKTReqRoomChat>(packet.BodyData);

                Room room = GetRoom(user.roomNumber);

                NotifyRoomChat(user.userId, requestData.Message, room);
            }
            catch(Exception ex)
            {
                MainServer.MainLogger.Error(ex.ToString());
            }
        }
        void RoomChatRespond(ERROR_CODE errorCode, string sessionId)
        {
            PKTResRoomChat roomEnterRes = new PKTResRoomChat();
            roomEnterRes.Result = (int)errorCode;

            var bodyData = MemoryPackSerializer.Serialize(roomEnterRes);
            var sendData = PacketToBytes.MakeBytes(PACKET_ID.ROOM_CHAT_RESPOND, bodyData);

            sendFunc(sessionId, sendData);
        }
        void NotifyRoomChat(string userId, string message, Room room)
        {
            PKTNTFRoomChat roomChatNTF = new PKTNTFRoomChat();
            roomChatNTF.UserId = userId;
            roomChatNTF.Message = message;

            var bodyData = MemoryPackSerializer.Serialize(roomChatNTF);
            var sendData = PacketToBytes.MakeBytes(PACKET_ID.ROOM_CHAT_NOTIFY, bodyData);

            room.Broadcast("", sendData);
        }
    }
}