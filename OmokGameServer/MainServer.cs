using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketBase.Logging;
using SuperSocket.SocketBase.Config;

using PacketDefine;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace OmokGameServer
{
    public class MainServer : AppServer<ClientSession, EFBinaryRequestInfo>, IHostedService
    {
        public static ILog MainLogger;
        public static int sessionCount;

        public static ServerOption serverOption;
        IServerConfig m_Config;

        private readonly IHostApplicationLifetime _appLifetime;

        PacketProcessor packetProcessor = new PacketProcessor();
        UserManager userManager = new UserManager();
        RoomManager roomManager = new RoomManager();

        public MainServer(IHostApplicationLifetime appLifetime, IOptions<ServerOption> serverConfig)
            :base(new DefaultReceiveFilterFactory<ReceiveFilter, EFBinaryRequestInfo>())
        {
            serverOption = serverConfig.Value;
            _appLifetime = appLifetime;

            NewSessionConnected += new SessionHandler<ClientSession>(OnClientConnect);
            SessionClosed += new SessionHandler<ClientSession, CloseReason>(OnClientDisConnect);
            NewRequestReceived += new RequestHandler<ClientSession, EFBinaryRequestInfo>(OnPacketReceived);
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
                    MainLogger = base.Logger;
                    MainLogger.Info("서버 초기화 성공");
                }

                Start();
                Settings();

                MainLogger.Info("서버 생성 성공");
            }
            catch(Exception ex)
            {
                Console.WriteLine($"[Error] 서버 생성 실패 : {ex}");
            }
        }
        public void StopServer()
        {
            Stop();

            // 패킷 프로세서 삭제
            packetProcessor.ProcessorStop();
        }

        public void Settings()
        {
            packetProcessor.ProcessorStart(userManager, roomManager);
            userManager.SetMaxUserNumber(serverOption.RoomMaxCount * serverOption.RoomMaxUserCount);
            roomManager.CreateRooms();
            PacketHandler.sendFunc = SendData;
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
                MainLogger.Error($"Send Error : {ex}");

                session.SendEndWhenSendingTimeOut();
                session.Close();
            }
            return true;
        }

        public void PassToProcessor(ServerPacketData packet)
        {
            packetProcessor.InsertPacket(packet);
        }

        void OnClientConnect(ClientSession clientSession)
        {
            MainLogger.Info($"New Session : {clientSession.SessionID}");

            ServerPacketData packet = new ServerPacketData();
            packet.SetPacketNoBody(clientSession.SessionID, (Int16)PACKET_ID.IN_NTF_CLIENT_CONNECT);
            
            PassToProcessor(packet);

            sessionCount++;
        }
        void OnClientDisConnect(ClientSession clientSession, CloseReason reason)
        {
            MainLogger.Info($"Closed Session : {clientSession.SessionID}");

            ServerPacketData packet = new ServerPacketData();
            packet.SetPacketNoBody(clientSession.SessionID, (Int16)PACKET_ID.IN_NTF_CLIENT_DISCONNECT);

            PassToProcessor(packet);

            sessionCount--;
        }
        void OnPacketReceived(ClientSession clientSession, EFBinaryRequestInfo requestInfo)
        {
            MainLogger.Info($"New Packet Received : sessionId : {clientSession.SessionID}");

            // 받은 세션과 reqInfo로 ServerPacketData로 만들어서 프로세서에 전달
            ServerPacketData packet = new ServerPacketData();
            packet.PacketSize = requestInfo.Size;
            packet.SessionId = clientSession.SessionID;
            packet.PacketId = requestInfo.PacketId;
            packet.BodyData = requestInfo.Body;

            PassToProcessor(packet);
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

    public class ClientSession : AppSession<ClientSession, EFBinaryRequestInfo>
    {

    }
}