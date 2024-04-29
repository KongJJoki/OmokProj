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
        IServerConfig m_Config;

        private readonly IHostApplicationLifetime _appLifetime;

        PacketProcessor packetProcessor = new PacketProcessor();
        UserManager userManager = new UserManager();
        RoomManager roomManager = new RoomManager();

        public MainServer(IHostApplicationLifetime appLifetime, IOptions<ServerOption> serverConfig)
            :base(new DefaultReceiveFilterFactory<ReceiveFilter, OmokBinaryRequestInfo>())
        {
            serverOption = serverConfig.Value;
            _appLifetime = appLifetime;

            NewSessionConnected += new SessionHandler<ClientSession>(OnClientConnect);
            SessionClosed += new SessionHandler<ClientSession, CloseReason>(OnClientDisConnect);
            NewRequestReceived += new RequestHandler<ClientSession, OmokBinaryRequestInfo>(OnPacketReceived);
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

                packetProcessor.ProcessorStart(userManager, roomManager, mainLogger, serverOption, SendData);
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
            userManager.SetMaxUserNumber(serverOption.RoomMaxCount * serverOption.RoomMaxUserCount);
            roomManager.SetServerOption(serverOption);
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

            // 받은 세션과 reqInfo로 ServerPacketData로 만들어서 프로세서에 전달
            ServerPacketData packet = new ServerPacketData();
            packet.PacketSize = requestInfo.Size;
            packet.SessionId = clientSession.SessionID;
            packet.PacketId = requestInfo.PacketId;
            packet.BodyData = requestInfo.Body;

            PassPacketToProcessor(packet);
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