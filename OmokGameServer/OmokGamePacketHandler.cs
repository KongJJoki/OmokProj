using MemoryPack;
using PacketDefine;
using InPacketTypes;
using SockInternalPacket;
using GameServerClientShare;
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
                    OmokStonePlaceRespond(SockErrorCode.OmokStonePlaceFailInvalidUser, sessionId);
                    return;
                }

                var room = roomManager.GetRoom(user.roomNumber);
                if (!room.isGameStart)
                {
                    OmokStonePlaceRespond(SockErrorCode.OmokStonePlaceFailGameNotStart, sessionId);
                    return;
                }

                var requestData = MemoryPackSerializer.Deserialize<PKTReqOmokStonePlace>(packet.bodyData);

                var omokBoard = room.GetOmokBoard();

                if(omokBoard.CheckStoneExist(requestData.PosX, requestData.PosY))
                {
                    OmokStonePlaceRespond(SockErrorCode.OmokStonePlaceFailAlreadyStoneExist, sessionId);
                    return;
                }

                omokBoard.OmokStonePlace(user.uid, requestData.PosX, requestData.PosY);
                OmokStonePlaceRespond(SockErrorCode.None, sessionId);
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

        void OmokStonePlaceRespond(SockErrorCode errorCode, string sessionId)
        {
            PKTResOmokStonePlace omokStonePlaceRes = new PKTResOmokStonePlace();
            omokStonePlaceRes.Result = errorCode;

            var bodyData = MemoryPackSerializer.Serialize(omokStonePlaceRes);
            var sendData = PacketToBytes.MakeToPacket(PACKET_ID.OmokStonePlaceResponse, bodyData);

            sendFunc(sessionId, sendData);
        }
    }
}