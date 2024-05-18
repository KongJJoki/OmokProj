using MemoryPack;
using PacketDefine;
using PacketTypes;

namespace OmokGameServer
{
    public class RoomPacketHandler : PacketHandler
    {
        public void SetPacketHandler(Dictionary<int, Action<ServerPacketData>> packetHandlerDictionary)
        {
            packetHandlerDictionary.Add((int)PACKET_ID.RoomEnterRequest, RoomEnterRequest);
            packetHandlerDictionary.Add((int)PACKET_ID.RoomLeaveRequest, RoomLeaveRequest);
            packetHandlerDictionary.Add((int)PACKET_ID.RoomChatRequest, RoomChatRequest);
        }
        public void RoomEnterRequest(ServerPacketData packet)
        {
            string sessionId = packet.sessionId;
            mainLogger.Debug($"SessionId({sessionId}) Request Room Enter");

            try
            {
                var user = userManager.GetUser(sessionId);

                if(user == null)
                {
                    RoomEnterRespond(ERROR_CODE.RoomEnterFailInvalidUser, sessionId);
                    return;
                }

                if(user.isInRoom)
                {
                    RoomEnterRespond(ERROR_CODE.RoomEnterFailAlreadyInRoom, sessionId);
                    return;
                }

                var requestData = MemoryPackSerializer.Deserialize<PKTReqRoomEnter>(packet.bodyData);

                var room = roomManager.GetRoom(requestData.RoomNumber);

                if(room == null)
                {
                    RoomEnterRespond(ERROR_CODE.RoomEnterFailNotExistRoom, sessionId);
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

                room.NotifyRoomMemberList(sessionId);
                room.NotifyNewUserRoomEnter(user);

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
            roomEnterRes.Result = errorCode;

            var bodyData = MemoryPackSerializer.Serialize(roomEnterRes);
            var sendData = PacketToBytes.MakeToPacket(PACKET_ID.RoomEnterResponse, bodyData);

            sendFunc(sessionId, sendData);
        }

        public void RoomLeaveRequest(ServerPacketData packet)
        {
            string sessionId = packet.sessionId;
            mainLogger.Debug($"SessionId({sessionId}) Request Room Leave");

            try
            {
                var user = userManager.GetUser(sessionId);

                if (user == null)
                {
                    RoomLeaveRespond(ERROR_CODE.RoomLeaveFailInvalidUser, sessionId);
                    return;
                }

                if(!user.isInRoom)
                {
                    RoomLeaveRespond(ERROR_CODE.RoomLeaveFailNotInRoom, sessionId);
                    return;
                }

                var requestData = MemoryPackSerializer.Deserialize<PKTReqRoomEnter>(packet.bodyData);

                var room = roomManager.GetRoom(user.roomNumber);

                if(requestData.RoomNumber != user.roomNumber)
                {
                    RoomLeaveRespond(ERROR_CODE.RoomLeaveFailNotInRoom, sessionId);
                    return;
                }

                ERROR_CODE errorCode = room.RemoveUser(user);
                if(errorCode!=ERROR_CODE.None)
                {
                    RoomLeaveRespond(errorCode, sessionId);
                    return;
                }

                //user.SetRoomLeave();

                room.NotifyRoomLeave(user);
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
            roomLeaveRes.Result = errorCode;

            var bodyData = MemoryPackSerializer.Serialize(roomLeaveRes);
            var sendData = PacketToBytes.MakeToPacket(PACKET_ID.RoomLeaveResponse, bodyData);

            sendFunc(sessionId, sendData);
        }

        public void RoomChatRequest(ServerPacketData packet)
        {
            string sessionId = packet.sessionId;
            mainLogger.Info($"sessionId({sessionId} send Chat)");

            try
            {
                var user = userManager.GetUser(sessionId);
                if(user == null)
                {
                    RoomChatRespond(ERROR_CODE.RoomChatFailInvalidUser, sessionId);
                    return;
                }

                if(!user.isInRoom)
                {
                    RoomChatRespond(ERROR_CODE.RoomChatFailNotInRoom, sessionId);
                    return;
                }

                RoomChatRespond(ERROR_CODE.None, sessionId);

                var requestData = MemoryPackSerializer.Deserialize<PKTReqRoomChat>(packet.bodyData);

                var room = roomManager.GetRoom(user.roomNumber);

                room.NotifyRoomChat(user.uid, requestData.Message);
            }
            catch(Exception ex)
            {
                mainLogger.Error(ex.ToString());
            }
        }
        void RoomChatRespond(ERROR_CODE errorCode, string sessionId)
        {
            PKTResRoomChat roomEnterRes = new PKTResRoomChat();
            roomEnterRes.Result = errorCode;

            var bodyData = MemoryPackSerializer.Serialize(roomEnterRes);
            var sendData = PacketToBytes.MakeToPacket(PACKET_ID.RoomChatResponse, bodyData);

            sendFunc(sessionId, sendData);
        }
    }
}