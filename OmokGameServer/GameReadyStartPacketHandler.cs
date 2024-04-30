using MemoryPack;
using PacketDefine;
using PacketTypes;

namespace OmokGameServer
{
    public class GameReadyStartPacketHandler : PacketHandler
    {
        public void SetPacketHandler(Dictionary<int, Action<ServerPacketData>> packetHandlerDictionary)
        {
            packetHandlerDictionary.Add((int)PACKET_ID.GameReadyRequest, GameReadyRequest);
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

        public void GameReadyRequest(ServerPacketData packet)
        {
            string sessionId = packet.SessionId;
            mainLogger.Debug($"sessionId({sessionId}) request Game Ready");

            try
            {
                var user = userManager.GetUser(sessionId);
                if(user == null)
                {
                    GameReadyResponse(ERROR_CODE.GameReadyFailInvalidUser, sessionId);
                    return;
                }

                if(!user.isInRoom)
                {
                    GameReadyResponse(ERROR_CODE.GameReadyFailNotInRoom, sessionId);
                    return;
                }

                var room = GetRoom(user.roomNumber);
                if(room.isGameStart)
                {
                    GameReadyResponse(ERROR_CODE.GameReadyFailAlreadyGameStart, sessionId);
                    return;
                }

                var myReadyStatus = room.GetReadyStatus(user);
                // 준비 취소 기능은 없다고 가정
                if(myReadyStatus)
                {
                    GameReadyResponse(ERROR_CODE.GameReadyFailAlreadyReadyStatus, sessionId);
                    return;
                }

                room.SetReadyStatus(user, true);
                GameReadyResponse(ERROR_CODE.None, sessionId);

                mainLogger.Info($"sessionId({sessionId}) Game Ready");
            }
            catch(Exception ex)
            {
                mainLogger.Error(ex.ToString());
            }
        }

        public void GameReadyResponse(ERROR_CODE errorCode, string sessionId)
        {
            PKTResGameReady gameReadyRes = new PKTResGameReady();
            gameReadyRes.Result = errorCode;

            var bodyData = MemoryPackSerializer.Serialize(gameReadyRes);
            var sendData = PacketToBytes.MakeBytes(PACKET_ID.GameReadyResponse, bodyData);

            sendFunc(sessionId, sendData);
        }
    }
}