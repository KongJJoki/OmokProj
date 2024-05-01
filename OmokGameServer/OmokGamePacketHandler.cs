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

                var room = roomList[user.roomNumber];
                if(!room.isGameStart)
                {
                    OmokStonePlaceRespond(ERROR_CODE.OmokStonePlaceFailGameNotStart, sessionId);
                    return;
                }

                var requestData = MemoryPackSerializer.Deserialize<PKTReqOmokStonePlace>(packet.bodyData);

                var omokBoard = room.GetOmokBoard();

                if(omokBoard.CheckStoneExist(requestData.PosX, requestData.PosY))
                {
                    OmokStonePlaceRespond(ERROR_CODE.OmokStonePlaceFailAlreadyStoneExist, sessionId);
                    return;
                }

                omokBoard.OmokStonePlace(user.userId, requestData.PosX, requestData.PosY);
                OmokStonePlaceRespond(ERROR_CODE.None, sessionId);
                room.NotifyOmokStonePlace(requestData.PosX, requestData.PosY);

                if(omokBoard.OmokWinCheck(requestData.PosX, requestData.PosY))
                {
                    room.NotifyOmokWin(user.userId);
                }
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

        public void TimeOutTurnChangeNotify(int roomNum)
        {
            var room = roomList[roomNum];

            room.TimeOutTurnChangeNotify();
        }
    }
}