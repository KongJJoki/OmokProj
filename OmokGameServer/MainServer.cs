using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketBase.Logging;
using SuperSocket.SocketBase.Config;

using PacketDefine;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace OmokGameServer
{
    public class MainServer : AppServer<ClientSession, OmokBinaryRequestInfo>, IHostedService
    {
        ILog mainLogger;

        ServerOption serverOption;
        DBConfig dbConfig;
        IServerConfig m_Config;

        private readonly IHostApplicationLifetime _appLifetime;

        PacketProcessor packetProcessor = new PacketProcessor();
        GameDBProcessor gameDBProcessor = new GameDBProcessor();
        UserManager userManager = new UserManager();
        RoomManager roomManager = new RoomManager();

        GameDB gameDB;

        public MainServer(IHostApplicationLifetime appLifetime, IOptions<ServerOption> serverConfig, IOptions<DBConfig> dbConfig)
            :base(new DefaultReceiveFilterFactory<ReceiveFilter, OmokBinaryRequestInfo>())
        {
            serverOption = serverConfig.Value;
            this.dbConfig = dbConfig.Value;
            _appLifetime = appLifetime;

            NewSessionConnected += new SessionHandler<ClientSession>(OnClientConnect);
            SessionClosed += new SessionHandler<ClientSession, CloseReason>(OnClientDisConnect);
            NewRequestReceived += new RequestHandler<ClientSession, OmokBinaryRequestInfo>(OnPacketReceived);

            packetProcessor.omokGamePacketHandler.gameFinish += gameDBProcessor.HandleGameResultUpdate;
        }

        public void InitConfig(ServerOption options)
        {
            serverOption = options;

            m_Config = new ServerConfig
            {
                Name = options.Name,
                Ip = "Any",
                Port = options.Port,
                Mode = SocketMode.Tcp,
                MaxConnectionNumber = options.MaxConnectionNumber,
                MaxRequestLength = options.MaxRequestLength,
                ReceiveBufferSize = options.ReceiveBufferSize,
                SendBufferSize = options.SendBufferSize
            };
        }

        public void StartServer()
        {
            try
            {
                bool isSuccess = Setup(new RootConfig(), m_Config, logFactory: new NLogLogFactory());

                if(!isSuccess)
                {
                    Console.WriteLine("[Error] 서버 네트워크 설정 실패");
                    return;
                }
                else
                {
                    mainLogger = base.Logger;
                    mainLogger.Info("서버 초기화 성공");
                }

                gameDB = new GameDB(mainLogger, dbConfig);
                packetProcessor.ProcessorStart(userManager, roomManager, mainLogger, serverOption, SendData);
                gameDBProcessor.ProcessorStart(mainLogger, gameDB);
                Settings();
                base.Start();

                mainLogger.Info("서버 생성 성공");
            }
            catch(Exception ex)
            {
                Console.WriteLine($"[Error] 서버 생성 실패 : {ex}");
            }
        }
        public void StopServer()
        {
            Stop();

            packetProcessor.ProcessorStop();
        }

        public void Settings()
        {
            userManager.Init(serverOption, PassPacketToProcessor, CloseConnection);
            roomManager.Init(serverOption, PassPacketToProcessor);
            roomManager.CreateRooms();
            Room.sendFunc = SendData;
        }

        public bool SendData(string sessionID, byte[] sendData)
        {
            var session = GetSessionByID(sessionID);

            try
            {
                if (session == null)
                {
                    return false;
                }

                session.Send(sendData, 0, sendData.Length);
            }
            catch (Exception ex)
            {
                mainLogger.Error($"Send Error : {ex}");

                session.SendEndWhenSendingTimeOut();
                session.Close();
            }
            return true;
        }

        public void PassPacketToProcessor(ServerPacketData packet)
        {
            packetProcessor.InsertPacket(packet);
        }

        void OnClientConnect(ClientSession clientSession)
        {
            mainLogger.Info($"New Session : {clientSession.SessionID} / Now Session Count : {SessionCount}");

            ServerPacketData packet = new ServerPacketData();

            packet.SetPacketNoBody(clientSession.SessionID, (Int16)PACKET_ID.InNTFClientConnect);

            PassPacketToProcessor(packet);
        }
        void OnClientDisConnect(ClientSession clientSession, CloseReason reason)
        {
            mainLogger.Info($"Closed Session : {clientSession.SessionID} /  Now Session Count : {SessionCount}");

            ServerPacketData packet = new ServerPacketData();

            packet.SetPacketNoBody(clientSession.SessionID, (Int16)PACKET_ID.InNTFClientDisconnect);

            PassPacketToProcessor(packet);
        }
        void OnPacketReceived(ClientSession clientSession, OmokBinaryRequestInfo requestInfo)
        {
            mainLogger.Info($"New Packet Received : sessionId : {clientSession.SessionID}");

            ServerPacketData packet = new ServerPacketData();
            packet.packetSize = requestInfo.Size;
            packet.sessionId = clientSession.SessionID;
            packet.packetId = requestInfo.PacketId;
            packet.bodyData = requestInfo.Body;

            PassPacketToProcessor(packet);
        }

        bool CloseConnection(string sessionId)
        {
            bool isCloseSuccess = false;

            var sessions = GetAllSessions();

            foreach(var session in sessions)
            {
                if(sessionId == session.SessionID)
                {
                    session.Close();
                    isCloseSuccess = true;
                    break;
                }
            }

            return isCloseSuccess;
        }

        public void AppOnStarted()
        {
            InitConfig(serverOption);
            StartServer();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _appLifetime.ApplicationStarted.Register(AppOnStarted);
            _appLifetime.ApplicationStopped.Register(StopServer);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {

            return Task.CompletedTask;
        }
    }

    public class ClientSession : AppSession<ClientSession, OmokBinaryRequestInfo>
    {

    }
}