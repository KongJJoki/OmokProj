using SuperSocket.SocketBase.Logging;
using System;
using System.Data;
using System.Threading.Tasks.Dataflow;
using MySql.Data.MySqlClient;
using SqlKata.Execution;
using SqlKata.Compilers;

namespace OmokGameServer
{
    class DBProcessor
    {
        ILog mainLogger;
        DBConfig dbConfig;
        ServerOption serverOption;
        MySqlCompiler compiler;

        bool isdbPacketProcessorRunning;
        Thread[] dbProcessorThList;

        BufferBlock<ServerPacketData> dbPacketBuffer = new BufferBlock<ServerPacketData>();

        Dictionary<int, Action<ServerPacketData, QueryFactory>> dbPacketHandlerDictionary = new Dictionary<int, Action<ServerPacketData, QueryFactory>>();

        DBGameResultSavePacketHandler dbGameResultPacketHandler = new DBGameResultSavePacketHandler();

        public void ProcessorStart(ILog mainLogger, DBConfig dbConfig, ServerOption serverOption)
        {
            this.mainLogger = mainLogger;
            this.dbConfig = dbConfig;
            this.serverOption = serverOption;
            compiler = new MySqlCompiler();

            isdbPacketProcessorRunning = true;
            dbProcessorThList = new Thread[serverOption.DBProcessThreadCount];

            SetPacketHandlers();

            for(int i = 0; i < serverOption.DBProcessThreadCount; i++)
            {
                dbProcessorThList[i] = new Thread(Process);
                dbProcessorThList[i].Start();
            }
        }

        public void ProcessorStop()
        {
            isdbPacketProcessorRunning = false;
            dbPacketBuffer.Complete();
        }

        public void InsertPacket(ServerPacketData packetData)
        {
            dbPacketBuffer.Post(packetData);
        }

        void SetPacketHandlers()
        {
            dbGameResultPacketHandler.Init(mainLogger);
            dbGameResultPacketHandler.SetPacketHandler(dbPacketHandlerDictionary);
        }

        void Process()
        {
            IDbConnection dbConnection;
            dbConnection = new MySqlConnection(dbConfig.GameDB);
            dbConnection.Open();

            QueryFactory queryFactory = new QueryFactory(dbConnection, compiler);

            while (isdbPacketProcessorRunning)
            {
                try
                {
                    var newPacket = dbPacketBuffer.Receive();

                    if (dbPacketHandlerDictionary.ContainsKey(newPacket.packetId))
                    {
                        dbPacketHandlerDictionary[newPacket.packetId](newPacket, queryFactory);
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