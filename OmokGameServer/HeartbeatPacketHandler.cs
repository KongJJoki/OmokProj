using MemoryPack;
using PacketDefine;
using PacketTypes;
using SuperSocket.SocketBase.Logging;

namespace OmokGameServer
{
    public class HeartBeatPacketHandler : PacketHandler
    {
        TimeSpan heartBeatLimitTime;
        public int heartBeatCheckUserIndexOffset;
        public int oneCheckCount;
        int maxConnectionCount;
        Func<string, bool> closeConnectionFunc;

        public new void Init(UserManager userManager, RoomManager roomManager, ILog mainLogger, ServerOption serverOption, Func<string, byte[], bool> sendFunc, Func<string, bool> closeConnectionFunc)
        {
            base.Init(userManager, roomManager, mainLogger, serverOption, sendFunc);

            heartBeatLimitTime = new TimeSpan(0, 0, 0, serverOption.HeartBeatTimeLimitSecond);
            heartBeatCheckUserIndexOffset = 0;
            maxConnectionCount = serverOption.MaxConnectionNumber;
            oneCheckCount = (int)Math.Ceiling((double)maxConnectionCount / serverOption.TotalDivideNumber);
            this.closeConnectionFunc = closeConnectionFunc;
        }

        public void SetPacketHandler(Dictionary<int, Action<ServerPacketData>> packetHandlerDictionary)
        {
            packetHandlerDictionary.Add((int)PACKET_ID.InNTFCheckHeartBeat, HeartBeatCheckRequest);
            packetHandlerDictionary.Add((int)PACKET_ID.HeartBeatResponseFromClient, HeartBeatResponseFromClient);
        }

        public void HeartBeatRequestToClient(string sessionId)
        {
            PKTHeartBeatToClient heartBeatToClient = new PKTHeartBeatToClient();

            var bodyData = MemoryPackSerializer.Serialize(heartBeatToClient);
            var sendData = PacketToBytes.MakeBytes(PACKET_ID.HeartBeatRequestToClient, bodyData);

            sendFunc(sessionId, sendData);
        }

        public void HeartBeatResponseFromClient(ServerPacketData packet)
        {
            mainLogger.Debug($"{packet.sessionId} heartBeat Arrive");
            User user = userManager.GetUser(packet.sessionId);
            user.lastHeartBeatTime = DateTime.Now;
        }

        public void HeartBeatCheckRequest(ServerPacketData packet)
        {
            for (int i = 0; i < oneCheckCount; i++)
            {
                if (heartBeatCheckUserIndexOffset == maxConnectionCount)
                {
                    break;
                }

                if(userManager.CheckUserArrayIsNull(heartBeatCheckUserIndexOffset))
                {
                    heartBeatCheckUserIndexOffset++;
                    continue;
                }

                TimeSpan timeDiff = userManager.CheckHeartBeatTimeDiff(heartBeatCheckUserIndexOffset);
                if (timeDiff > heartBeatLimitTime)
                {
                    string sessionId = userManager.GetUserSessionIdByheartBeatIndex(heartBeatCheckUserIndexOffset);
                    // 하트비트 시간 넘은거 알리기
                    // 일단 그냥 끊어버리기
                    closeConnectionFunc(sessionId);
                    userManager.RemoveUserFromArray(heartBeatCheckUserIndexOffset);
                    mainLogger.Debug($"force Disconnected");
                }
                else
                {
                    HeartBeatRequestToClient(packet.sessionId);
                }

                heartBeatCheckUserIndexOffset++;
            }

            if (heartBeatCheckUserIndexOffset >= maxConnectionCount)
            {
                heartBeatCheckUserIndexOffset = 0;
            }
        }
    }
}