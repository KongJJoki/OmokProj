using MemoryPack;
using PacketDefine;
using InPacketTypes;
using SockInternalPacket;
using GameServerClientShare;

namespace OmokGameServer
{
    public class RoomPacketHandler : PacketHandler
    {
        public void SetPacketHandler(Dictionary<int, Action<ServerPacketData>> packetHandlerDictionary)
        {
            packetHandlerDictionary.Add((int)PacketID.RoomEnterRequest, RoomEnterRequest);
            packetHandlerDictionary.Add((int)PacketID.RoomLeaveRequest, RoomLeaveRequest);
            packetHandlerDictionary.Add((int)PacketID.RoomChatRequest, RoomChatRequest);
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
                    RoomEnterRespond(SockErrorCode.RoomEnterFailInvalidUser, sessionId);
                    return;
                }

                if(user.isInRoom)
                {
                    RoomEnterRespond(SockErrorCode.RoomEnterFailAlreadyInRoom, sessionId);
                    return;
                }

                var requestData = MemoryPackSerializer.Deserialize<PKTReqRoomEnter>(packet.bodyData);

                var room = roomManager.GetRoom(requestData.RoomNumber);

                if(room == null)
                {
                    RoomEnterRespond(SockErrorCode.RoomEnterFailNotExistRoom, sessionId);
                    return;
                }

                SockErrorCode errorCode = room.AddUser(user);
                if(errorCode != SockErrorCode.None)
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

        void RoomEnterRespond(SockErrorCode errorCode, string sessionId)
        {
            PKTResRoomEnter roomEnterRes = new PKTResRoomEnter();
            roomEnterRes.Result = errorCode;

            var bodyData = MemoryPackSerializer.Serialize(roomEnterRes);
            var sendData = PacketToBytes.MakeToPacket(PacketID.RoomEnterResponse, bodyData);

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
                    RoomLeaveRespond(SockErrorCode.RoomLeaveFailInvalidUser, sessionId);
                    return;
                }

                if(!user.isInRoom)
                {
                    RoomLeaveRespond(SockErrorCode.RoomLeaveFailNotInRoom, sessionId);
                    return;
                }

                var requestData = MemoryPackSerializer.Deserialize<PKTReqRoomEnter>(packet.bodyData);

                var room = roomManager.GetRoom(user.roomNumber);

                if(requestData.RoomNumber != user.roomNumber)
                {
                    RoomLeaveRespond(SockErrorCode.RoomLeaveFailNotInRoom, sessionId);
                    return;
                }

                SockErrorCode errorCode = room.RemoveUser(user);
                if(errorCode!=SockErrorCode.None)
                {
                    RoomLeaveRespond(errorCode, sessionId);
                    return;
                }

                room.NotifyRoomLeave(user);
                RoomLeaveRespond(errorCode, sessionId);

                mainLogger.Info($"sessionId({sessionId}) leave Room");
            }
            catch(Exception ex)
            {
                mainLogger.Error(ex.ToString());
            }
        }

        void RoomLeaveRespond(SockErrorCode errorCode, string sessionId)
        {
            PKTResRoomLeave roomLeaveRes = new PKTResRoomLeave();
            roomLeaveRes.Result = errorCode;

            var bodyData = MemoryPackSerializer.Serialize(roomLeaveRes);
            var sendData = PacketToBytes.MakeToPacket(PacketID.RoomLeaveResponse, bodyData);

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
                    RoomChatRespond(SockErrorCode.RoomChatFailInvalidUser, sessionId);
                    return;
                }

                if(!user.isInRoom)
                {
                    RoomChatRespond(SockErrorCode.RoomChatFailNotInRoom, sessionId);
                    return;
                }

                RoomChatRespond(SockErrorCode.None, sessionId);

                var requestData = MemoryPackSerializer.Deserialize<PKTReqRoomChat>(packet.bodyData);

                var room = roomManager.GetRoom(user.roomNumber);

                room.NotifyRoomChat(user.uid, requestData.Message);
            }
            catch(Exception ex)
            {
                mainLogger.Error(ex.ToString());
            }
        }
        void RoomChatRespond(SockErrorCode errorCode, string sessionId)
        {
            PKTResRoomChat roomEnterRes = new PKTResRoomChat();
            roomEnterRes.Result = errorCode;

            var bodyData = MemoryPackSerializer.Serialize(roomEnterRes);
            var sendData = PacketToBytes.MakeToPacket(PacketID.RoomChatResponse, bodyData);

            sendFunc(sessionId, sendData);
        }
    }
}