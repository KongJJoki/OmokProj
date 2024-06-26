using PacketDefine;
using GameServerClientShare;
using InPacketTypes;
using SockInternalPacket;
using SuperSocket.SocketBase.Logging;
using System.Threading.Tasks.Dataflow;

namespace OmokGameServer
{
    class PacketProcessor
    {
        ILog mainLogger;
        ServerOption serverOption;
        Func<string, byte[], bool> sendFunc;
        Action<ServerPacketData> passPacketToDBProcessor;
        Action<ServerPacketData> passPacketToRedisProcessor;

        bool isProcessorRunning = false;
        Thread packetProcessorTh = null;

        BufferBlock<ServerPacketData> packetBuffer = new BufferBlock<ServerPacketData>();

        UserManager userManager;
        RoomManager roomManager;

        Dictionary<int, Action<ServerPacketData>> packetHandlerDictionary = new Dictionary<int, Action<ServerPacketData>>();

        ConnectionLoginPacketHandler loginPacketHandler = new ConnectionLoginPacketHandler();
        RoomPacketHandler roomPacketHandler = new RoomPacketHandler();
        GameReadyStartPacketHandler gameReadyStartPacketHandler = new GameReadyStartPacketHandler();
        OmokGamePacketHandler omokGamePacketHandler = new OmokGamePacketHandler();
        RoomCheckPacketHandler turnCheckPacketHandler = new RoomCheckPacketHandler();
        HeartBeatPacketHandler heartBeatPacketHandler = new HeartBeatPacketHandler();

        public void ProcessorStart(UserManager userManager, RoomManager roomManager, ILog mainLogger, ServerOption serverOption, Func<string, byte[], bool> sendFunc, Action<ServerPacketData> passPacketToDBProcessor, Action<ServerPacketData> passPacketToRedisProcessor)
        {
            this.userManager = userManager;
            this.roomManager = roomManager;
            this.mainLogger = mainLogger;
            this.serverOption = serverOption;
            this.sendFunc = sendFunc;
            this.passPacketToDBProcessor = passPacketToDBProcessor;
            this.passPacketToRedisProcessor = passPacketToRedisProcessor;

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

            turnCheckPacketHandler.Init(userManager, roomManager, mainLogger, serverOption, sendFunc);
            turnCheckPacketHandler.SetPacketHandler(packetHandlerDictionary);

            heartBeatPacketHandler.Init(userManager, roomManager, mainLogger, serverOption, sendFunc);
            heartBeatPacketHandler.SetPacketHandler(packetHandlerDictionary);
        }

        void Process()
        {
            while (isProcessorRunning)
            {
                try
                {
                    var newPacket = packetBuffer.Receive();

                    if(newPacket.packetId == (int)InPacketID.InSaveGameResult)
                    {
                        passPacketToDBProcessor(newPacket);
                        continue;
                    }

                    if(newPacket.packetId == (int)PacketID.LoginRequest)
                    {
                        passPacketToRedisProcessor(newPacket);
                        continue;
                    }

                    if (packetHandlerDictionary.ContainsKey(newPacket.packetId))
                    {
                        packetHandlerDictionary[newPacket.packetId](newPacket);
                    }
                    else
                    {
                        mainLogger.Debug($"Unknown PacketId : {newPacket.packetId}"); ;
                    }
                }
                catch (Exception ex)
                {
                    mainLogger.Error($"PacketProcessor Error : {ex}");
                }
            }
        }
    }
}