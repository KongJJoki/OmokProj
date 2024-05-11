using MemoryPack;
using PacketDefine;
using PacketTypes;
using SuperSocket.SocketBase.Logging;

namespace OmokGameServer
{
    public class HeartBeatPacketHandler : PacketHandler
    {
        public int heartBeatCheckUserIndexOffset;
        int maxConnectionCount;
        public int oneCheckCount;

        public new void Init(UserManager userManager, RoomManager roomManager, ILog mainLogger, ServerOption serverOption, Func<string, byte[], bool> sendFunc)
        {
            base.Init(userManager, roomManager, mainLogger, serverOption, sendFunc);
            heartBeatCheckUserIndexOffset = 0;
            maxConnectionCount = serverOption.MaxConnectionNumber;
            oneCheckCount = (int)Math.Ceiling((double)maxConnectionCount / serverOption.TotalDivideNumber);
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
            var sendData = PacketToBytes.MakeToPacket(PACKET_ID.HeartBeatRequestToClient, bodyData);

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
                if (heartBeatCheckUserIndexOffset >= maxConnectionCount)
                {
                    heartBeatCheckUserIndexOffset = 0;
                    break;
                }

                bool isHeartBeatMeaningful = userManager.CheckHeartBeatMeaningful(heartBeatCheckUserIndexOffset);
                if (isHeartBeatMeaningful)
                {
                    HeartBeatRequestToClient(packet.sessionId);
                }

                heartBeatCheckUserIndexOffset++;
            }
        }
    }
}