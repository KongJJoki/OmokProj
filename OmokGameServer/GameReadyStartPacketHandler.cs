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
            packetHandlerDictionary.Add((int)PACKET_ID.GameStartRequest, GameStartRequest);
        }

        public void GameReadyRequest(ServerPacketData packet)
        {
            string sessionId = packet.sessionId;
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

                var room = roomManager.GetRoom(user.roomNumber);
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

        public void GameStartRequest(ServerPacketData packet)
        {
            string sessionId = packet.sessionId;
            mainLogger.Debug($"sessionId({sessionId}) requset Game Start");

            try
            {
                var user = userManager.GetUser(sessionId);
                if(user == null)
                {
                    GameStartResponse(ERROR_CODE.GameStartFailInvalidUser, sessionId);
                    return;
                }

                if(!user.isInRoom)
                {
                    GameStartResponse(ERROR_CODE.GameStartFailNotInRoom, sessionId);
                    return;
                }

                var room = roomManager.GetRoom(user.roomNumber);

                if (room.nowUserCount < 2)
                {
                    GameStartResponse(ERROR_CODE.GameStartFailNotEnoughUserCount, sessionId);
                    return;
                }

                if(room.isGameStart)
                {
                    GameStartResponse(ERROR_CODE.GameStartFailAlreadyGameStart, sessionId);
                    return;
                }

                if(!room.GetReadyStatus(user))
                {
                    GameStartResponse(ERROR_CODE.GameStartFailNotReady, sessionId);
                    return;
                }

                if(!room.CheckAllReady())
                {
                    GameStartResponse(ERROR_CODE.GameStartFailNotAllReady, sessionId);
                    return;
                }

                GameStartResponse(ERROR_CODE.None, sessionId);
                room.NotifyGameStart(user.userName);

                mainLogger.Info($"{room.RoomNumber} room Game Start");
            }
            catch(Exception ex)
            {
                mainLogger.Error(ex.ToString());
            }
        }

        public void GameStartResponse(ERROR_CODE errorCode, string sessionId)
        {
            PKTResGameStart gameStartRes = new PKTResGameStart();
            gameStartRes.Result = errorCode;

            var bodyData = MemoryPackSerializer.Serialize(gameStartRes);
            var sendData = PacketToBytes.MakeBytes(PACKET_ID.GameStartResponse, bodyData);

            sendFunc(sessionId, sendData);
        }
    }
}