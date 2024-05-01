using MemoryPack;
using PacketDefine;
using PacketTypes;

namespace OmokGameServer
{
    public class OmokGamePacketHandler : PacketHandler
    {
        public void SetPacketHandler(Dictionary<int, Action<ServerPacketData>> packetHandlerDictionary)
        {
            packetHandlerDictionary.Add((int)PACKET_ID.OmokStonePlaceRequest, OmokStonePlaceRequest);
            //packetHandlerDictionary.Add((int)PACKET_ID.GameStartRequest, GameStartRequest);
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

        public void OmokStonePlaceRequest(ServerPacketData packet)
        {
            string sessionId = packet.sessionId;
            mainLogger.Debug($"sessionId({sessionId}) request StonePlace");

            try
            {
                var user = userManager.GetUser(sessionId);

                if (user == null)
                {
                    OmokStonePlaceRespond(ERROR_CODE.OmokStonePlaceFailInvalidUser, sessionId);
                    return;
                }

                var room = GetRoom(user.roomNumber);
                if(!room.isGameStart)
                {
                    OmokStonePlaceRespond(ERROR_CODE.OmokStonePlaceFailGameNotStart, sessionId);
                    return;
                }

                var requestData = MemoryPackSerializer.Deserialize<PKTReqOmokStonePlace>(packet.bodyData);
                if(room.CheckStoneExist(requestData.PosX, requestData.PosY))
                {
                    OmokStonePlaceRespond(ERROR_CODE.OmokStonePlaceFailAlreadyStoneExist, sessionId);
                    return;
                }

                room.OmokStonePlace(user.userId, requestData.PosX, requestData.PosY);
                OmokStonePlaceRespond(ERROR_CODE.None, sessionId);
                room.NotifyOmokStonePlace(user.userId, requestData.PosX, requestData.PosY);
            }
            catch (Exception ex)
            {
                mainLogger.Error(ex.ToString());
            }
        }

        void OmokStonePlaceRespond(ERROR_CODE errorCode, string sessionId)
        {
            PKTResOmokStonePlace omokStonePlaceRes = new PKTResOmokStonePlace();
            omokStonePlaceRes.Result = errorCode;

            var bodyData = MemoryPackSerializer.Serialize(omokStonePlaceRes);
            var sendData = PacketToBytes.MakeBytes(PACKET_ID.OmokStonePlaceResponse, bodyData);

            sendFunc(sessionId, sendData);
        }
    }
}