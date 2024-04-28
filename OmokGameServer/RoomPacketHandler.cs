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

                user.SetRoomNumber(requestData.RoomNumber);

                RoomEnterRespond(errorCode, sessionId);

                MainServer.MainLogger.Info($"sessionId({sessionId}) enter {requestData.RoomNumber} Room");
            }
            catch(Exception ex)
            {
                MainServer.MainLogger.Error(ex.ToString());
            }
        }
        public Room GetRoom(int roomNum)
        {
            int roomIndex = roomNum - MainServer.serverOption.RoomStartNumber;
            
            if(roomIndex < MainServer.serverOption.RoomStartNumber - 1 || roomIndex > MainServer.serverOption.RoomMaxCount)
            {
                return null;
            }
            
            return roomList[roomIndex];
        }

        public void RoomEnterRespond(ERROR_CODE errorCode, string sessionId)
        {
            PKTResRoomEnter roomEnterRes = new PKTResRoomEnter();
            roomEnterRes.Result = (int)errorCode;

            var bodyData = MemoryPackSerializer.Serialize(roomEnterRes);
            var sendData = PacketToBytes.MakeBytes(PACKET_ID.ROOM_ENTER_RESPOND, bodyData);

            sendFunc(sessionId, sendData);
        }
    }
}