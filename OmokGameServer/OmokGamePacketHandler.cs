using MemoryPack;
using PacketDefine;
using PacketTypes;
using SuperSocket.Common;

namespace OmokGameServer
{
    public class OmokGamePacketHandler : PacketHandler
    {
        public void SetPacketHandler(Dictionary<int, Action<ServerPacketData>> packetHandlerDictionary)
        {
            packetHandlerDictionary.Add((int)PACKET_ID.OmokStonePlaceRequest, OmokStonePlaceRequest);
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

                var room = roomManager.GetRoom(user.roomNumber);
                if (!room.isGameStart)
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

                omokBoard.OmokStonePlace(user.uid, requestData.PosX, requestData.PosY);
                OmokStonePlaceRespond(ERROR_CODE.None, sessionId);
                room.NotifyOmokStonePlace(requestData.PosX, requestData.PosY);

                if(omokBoard.OmokWinCheck(requestData.PosX, requestData.PosY))
                {
                    var otherUser = room.GetOtherUser(user);
                    room.NotifyOmokWin(user);
                    room.NotifyOmokLose(room.GetOtherUser(user));

                    room.SaveGameResult(user, otherUser);

                    room.GameFinish();
                    roomManager.EnqueueEmptyRoom(room.RoomNumber);
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
            var sendData = PacketToBytes.MakeToPacket(PACKET_ID.OmokStonePlaceResponse, bodyData);

            sendFunc(sessionId, sendData);
        }
    }
}