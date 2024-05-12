using SuperSocket.SocketBase.Logging;
using System.Data;
using System.Threading.Tasks.Dataflow;
using MySql.Data.MySqlClient;
using SqlKata.Execution;
using SqlKata.Compilers;
using CloudStructures;

namespace OmokGameServer
{
    class RedisProcessor
    {
        ILog mainLogger;
        DBConfig dbConfig;
        ServerOption serverOption;
        RedisConfig redisConfig;
        Action<ServerPacketData> passPacketToPacketProcessor;

        bool isRedisPacketProcessorRunning;
        Thread[] redisProcessorThList;

        BufferBlock<ServerPacketData> redisPacketBuffer = new BufferBlock<ServerPacketData>();

        Dictionary<int, Func<ServerPacketData, RedisConnection, Task>> redisPacketHandlerDictionary = new Dictionary<int, Func<ServerPacketData, RedisConnection, Task>>();

        RedisLoginPacketHandler redisLoginPacketHandler = new RedisLoginPacketHandler();

        public void ProcessorStart(ILog mainLogger, DBConfig dbConfig, ServerOption serverOption, Action<ServerPacketData> passPacketToPacketProcessor)
        {
            this.mainLogger = mainLogger;
            this.dbConfig = dbConfig;
            this.serverOption = serverOption;
            redisConfig = new RedisConfig("redisDB", this.dbConfig.RedisDB);
            this.passPacketToPacketProcessor = passPacketToPacketProcessor;

            isRedisPacketProcessorRunning = true;
            redisProcessorThList = new Thread[serverOption.RedisProcessThreadCount];

            SetPacketHandlers();

            for (int i = 0; i < serverOption.RedisProcessThreadCount; i++)
            {
                redisProcessorThList[i] = new Thread(Process);
                redisProcessorThList[i].Start();
            }
        }

        public void ProcessorStop()
        {
            isRedisPacketProcessorRunning = false;
            redisPacketBuffer.Complete();
        }

        public void InsertPacket(ServerPacketData packetData)
        {
            redisPacketBuffer.Post(packetData);
        }

        void SetPacketHandlers()
        {
            redisLoginPacketHandler.Init(mainLogger, passPacketToPacketProcessor);
            redisLoginPacketHandler.SetPacketHandler(redisPacketHandlerDictionary);
        }

        void Process()
        {
            RedisConnection redisConnection;
            redisConnection = new RedisConnection(redisConfig);

            while (isRedisPacketProcessorRunning)
            {
                try
                {
                    var newPacket = redisPacketBuffer.Receive();

                    if (redisPacketHandlerDictionary.ContainsKey(newPacket.packetId))
                    {
                        redisPacketHandlerDictionary[newPacket.packetId](newPacket, redisConnection);
                    }
                    else
                    {
                        mainLogger.Debug($"Unknown PacketId : {newPacket.packetId}"); ;
                    }
                }
                catch (Exception ex)
                {
                    mainLogger.Error($"DBPacketProcessor Error : {ex}");
                }
            }
        }
    }
}