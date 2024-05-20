using MemoryPack;
using PacketDefine;
using InPacketTypes;
using GameServerClientShare;

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
                    GameReadyResponse(SockErrorCode.GameReadyFailInvalidUser, sessionId);
                    return;
                }

                if(!user.isInRoom)
                {
                    GameReadyResponse(SockErrorCode.GameReadyFailNotInRoom, sessionId);
                    return;
                }

                var room = roomManager.GetRoom(user.roomNumber);
                if(room.isGameStart)
                {
                    GameReadyResponse(SockErrorCode.GameReadyFailAlreadyGameStart, sessionId);
                    return;
                }

                var myReadyStatus = room.GetReadyStatus(user);
                // 준비 취소 기능은 없다고 가정
                if(myReadyStatus)
                {
                    GameReadyResponse(SockErrorCode.GameReadyFailAlreadyReadyStatus, sessionId);
                    return;
                }

                room.SetReadyStatus(user, true);
                GameReadyResponse(SockErrorCode.None, sessionId);

                mainLogger.Info($"sessionId({sessionId}) Game Ready");
            }
            catch(Exception ex)
            {
                mainLogger.Error(ex.ToString());
            }
        }

        public void GameReadyResponse(SockErrorCode errorCode, string sessionId)
        {
            PKTResGameReady gameReadyRes = new PKTResGameReady();
            gameReadyRes.Result = errorCode;

            var bodyData = MemoryPackSerializer.Serialize(gameReadyRes);
            var sendData = PacketToBytes.MakeToPacket(PACKET_ID.GameReadyResponse, bodyData);

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
                    GameStartResponse(SockErrorCode.GameStartFailInvalidUser, sessionId);
                    return;
                }

                if(!user.isInRoom)
                {
                    GameStartResponse(SockErrorCode.GameStartFailNotInRoom, sessionId);
                    return;
                }

                var room = roomManager.GetRoom(user.roomNumber);

                if (room.nowUserCount < 2)
                {
                    GameStartResponse(SockErrorCode.GameStartFailNotEnoughUserCount, sessionId);
                    return;
                }

                if(room.isGameStart)
                {
                    GameStartResponse(SockErrorCode.GameStartFailAlreadyGameStart, sessionId);
                    return;
                }

                if(!room.GetReadyStatus(user))
                {
                    GameStartResponse(SockErrorCode.GameStartFailNotReady, sessionId);
                    return;
                }

                if(!room.CheckAllReady())
                {
                    GameStartResponse(SockErrorCode.GameStartFailNotAllReady, sessionId);
                    return;
                }

                GameStartResponse(SockErrorCode.None, sessionId);
                room.NotifyGameStart(user.uid);

                mainLogger.Info($"{room.RoomNumber} room Game Start");
            }
            catch(Exception ex)
            {
                mainLogger.Error(ex.ToString());
            }
        }

        public void GameStartResponse(SockErrorCode errorCode, string sessionId)
        {
            PKTResGameStart gameStartRes = new PKTResGameStart();
            gameStartRes.Result = errorCode;

            var bodyData = MemoryPackSerializer.Serialize(gameStartRes);
            var sendData = PacketToBytes.MakeToPacket(PACKET_ID.GameStartResponse, bodyData);

            sendFunc(sessionId, sendData);
        }
    }
}