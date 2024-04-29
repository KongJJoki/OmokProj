using Antlr4.Runtime.Tree;
using MemoryPack;
using PacketDefine;
using PacketTypes;

namespace OmokGameServer
{
    public class RoomPacketHandler : PacketHandler
    {
        public void SetPacketHandler(Dictionary<int, Action<ServerPacketData>> packetHandlerDictionary)
        {
            packetHandlerDictionary.Add((int)PACKET_ID.RoomEnterRequset, RoomEnterRequest);
            packetHandlerDictionary.Add((int)PACKET_ID.RoomLeaveRequest, RoomLeaveRequest);
            packetHandlerDictionary.Add((int)PACKET_ID.RoomChatRequest, RoomChatRequest);
        }
        public Room GetRoom(int roomNum)
        {
            int roomIndex = roomNum - serverOption.RoomStartNumber;

            if (roomIndex < serverOption.RoomStartNumber - 1 || roomIndex > serverOption.RoomMaxCount)
            {
                return null;
            }

            return roomList[roomIndex];
        }
        public void RoomEnterRequest(ServerPacketData packet)
        {
            string sessionId = packet.SessionId;
            mainLogger.Info($"SessionId({sessionId}) Request Room Enter");

            try
            {
                var user = userManager.GetUser(sessionId);

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

                var room = GetRoom(requestData.RoomNumber);

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

                NotifyRoomMemberList(sessionId, room);
                NotifyNewUserRoomEnter(user, room);

                mainLogger.Info($"sessionId({sessionId}) enter {requestData.RoomNumber} Room");
            }
            catch(Exception ex)
            {
                mainLogger.Error(ex.ToString());
            }
        }

        void RoomEnterRespond(ERROR_CODE errorCode, string sessionId)
        {
            PKTResRoomEnter roomEnterRes = new PKTResRoomEnter();
            roomEnterRes.Result = (int)errorCode;

            var bodyData = MemoryPackSerializer.Serialize(roomEnterRes);
            var sendData = PacketToBytes.MakeBytes(PACKET_ID.RoomEnterRespond, bodyData);

            sendFunc(sessionId, sendData);
        }
        void NotifyNewUserRoomEnter(User user, Room room)
        {
            if(room.nowUserCount == 0)
            {
                return;
            }

            PKTNTFRoomEnter roomEnterNTF = new PKTNTFRoomEnter();
            roomEnterNTF.UserId = user.userId;

            var bodyData = MemoryPackSerializer.Serialize(roomEnterNTF);
            var sendData = PacketToBytes.MakeBytes(PACKET_ID.RoomEnterNotify, bodyData);

            room.Broadcast(user.sessionId, sendData);
        }
        void NotifyRoomMemberList(string sessionId, Room room)
        {
            if(room.nowUserCount == 0)
            {
                return;
            }

            PKTNTFRoomMember roomMemberNTF = new PKTNTFRoomMember();
            roomMemberNTF.UserIdList = room.GetUserIds();

            var bodyData = MemoryPackSerializer.Serialize(roomMemberNTF);
            var sendData = PacketToBytes.MakeBytes(PACKET_ID.RoomMemberNotify, bodyData);

            sendFunc(sessionId, sendData);
        }

        public void RoomLeaveRequest(ServerPacketData packet)
        {
            string sessionId = packet.SessionId;
            mainLogger.Info($"SessionId({sessionId}) Request Room Leave");

            try
            {
                var user = userManager.GetUser(sessionId);

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

                var room = GetRoom(user.roomNumber);

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

                mainLogger.Info($"sessionId({sessionId}) leave Room");
            }
            catch(Exception ex)
            {
                mainLogger.Error(ex.ToString());
            }
        }

        void RoomLeaveRespond(ERROR_CODE errorCode, string sessionId)
        {
            PKTResRoomLeave roomLeaveRes = new PKTResRoomLeave();
            roomLeaveRes.Result = (int)errorCode;

            var bodyData = MemoryPackSerializer.Serialize(roomLeaveRes);
            var sendData = PacketToBytes.MakeBytes(PACKET_ID.RoomLeaveRespond, bodyData);

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
            var sendData = PacketToBytes.MakeBytes(PACKET_ID.RoomLeaveNotify, bodyData);

            room.Broadcast("", sendData);
        }

        public void RoomChatRequest(ServerPacketData packet)
        {
            string sessionId = packet.SessionId;
            mainLogger.Info($"sessionId({sessionId} send Chat)");

            try
            {
                var user = userManager.GetUser(sessionId);
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

                var room = GetRoom(user.roomNumber);

                NotifyRoomChat(user.userId, requestData.Message, room);
            }
            catch(Exception ex)
            {
                mainLogger.Error(ex.ToString());
            }
        }
        void RoomChatRespond(ERROR_CODE errorCode, string sessionId)
        {
            PKTResRoomChat roomEnterRes = new PKTResRoomChat();
            roomEnterRes.Result = (int)errorCode;

            var bodyData = MemoryPackSerializer.Serialize(roomEnterRes);
            var sendData = PacketToBytes.MakeBytes(PACKET_ID.RoomChatRespond, bodyData);

            sendFunc(sessionId, sendData);
        }
        void NotifyRoomChat(string userId, string message, Room room)
        {
            PKTNTFRoomChat roomChatNTF = new PKTNTFRoomChat();
            roomChatNTF.UserId = userId;
            roomChatNTF.Message = message;

            var bodyData = MemoryPackSerializer.Serialize(roomChatNTF);
            var sendData = PacketToBytes.MakeBytes(PACKET_ID.RoomChatNotify, bodyData);

            room.Broadcast("", sendData);
        }
    }
}