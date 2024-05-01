using SuperSocket.SocketBase.Logging;
using System.Threading.Tasks.Dataflow;

namespace OmokGameServer
{
    class PacketProcessor
    {
        bool isProcessorRunning = false;
        Thread packetProcessorTh = null;
        ILog mainLogger;
        ServerOption serverOption;
        Func<string, byte[], bool> sendFunc;

        BufferBlock<ServerPacketData> packetBuffer = new BufferBlock<ServerPacketData>();

        UserManager userManager;
        RoomManager roomManager;

        // 방 번호 범위 튜플
        // 방 객체 리스트

        // 패킷 핸들러 맵 선언
        // 모든 함수들이 ServerPacketData를 매개변수로 받아서 쓰기 때문에 value가 Action<ServerPacketData>
        Dictionary<int, Action<ServerPacketData>> packetHandlerDictionary = new Dictionary<int, Action<ServerPacketData>>();

        ConnectionLoginPacketHandler loginPacketHandler = new ConnectionLoginPacketHandler();
        RoomPacketHandler roomPacketHandler = new RoomPacketHandler();
        GameReadyStartPacketHandler gameReadyStartPacketHandler = new GameReadyStartPacketHandler();
        OmokGamePacketHandler omokGamePacketHandler = new OmokGamePacketHandler();

        public void ProcessorStart(UserManager userManager, RoomManager roomManager, ILog mainLogger, ServerOption serverOption, Func<string, byte[], bool> sendFunc)
        {
            this.userManager = userManager;
            this.roomManager = roomManager;
            this.mainLogger = mainLogger;
            this.serverOption = serverOption;
            this.sendFunc = sendFunc;

            SetPacketHandlers();

            isProcessorRunning = true;
            packetProcessorTh = new Thread(Process);
            packetProcessorTh.Start();
        }

        public void ProcessorStop()
        {
            isProcessorRunning = false;
            packetBuffer.Complete();
        }

        public void InsertPacket(ServerPacketData packetData)
        {
            packetBuffer.Post(packetData);
        }

        void SetPacketHandlers()
        {
            loginPacketHandler.Init(userManager, roomManager, mainLogger, serverOption, sendFunc);
            loginPacketHandler.SetPacketHandler(packetHandlerDictionary);

            roomPacketHandler.Init(userManager, roomManager, mainLogger, serverOption, sendFunc);
            roomPacketHandler.SetPacketHandler(packetHandlerDictionary);

            gameReadyStartPacketHandler.Init(userManager, roomManager, mainLogger, serverOption, sendFunc);
            gameReadyStartPacketHandler.SetPacketHandler(packetHandlerDictionary);

            omokGamePacketHandler.Init(userManager, roomManager, mainLogger, serverOption, sendFunc);
            omokGamePacketHandler.SetPacketHandler(packetHandlerDictionary);
        }

        void Process()
        {
            while(isProcessorRunning)
            {
                try
                {
                    var newPacket = packetBuffer.Receive();

                    if(packetHandlerDictionary.ContainsKey(newPacket.packetId))
                    {
                        packetHandlerDictionary[newPacket.packetId](newPacket);
                    }
                    else
                    {
                        mainLogger.Debug($"Unknown PacketId : {newPacket.packetId}"); ;
                    }
                }
                catch(Exception ex)
                {
                    mainLogger.Error($"PacketProcessor Error : {ex.ToString()}");
                }
            }
        }

    }
}