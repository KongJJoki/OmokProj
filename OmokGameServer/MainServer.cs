using SuperSocket.SocketBase;
using SuperSocket.SocketEngine;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketBase.Logging;
using SuperSocket.SocketBase.Config;

using PacketDefine;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace OmokGameServer
{
    public class MainServer : AppServer<ClientSession, EFBinaryRequestInfo>
    {
        public static ILog MainLogger;

        public static ServerOption serverOption;
        IServerConfig m_Config;

        // 패킷 프로세서 선언
        PacketProcessor packetProcessor = new PacketProcessor();
        // 룸 매니저 선언

        public MainServer(IOptions<ServerOption> serverConfig)
            :base(new DefaultReceiveFilterFactory<ReceiveFilter, EFBinaryRequestInfo>())
        {
            serverOption = serverConfig.Value;

            // 새로운 세션 연결
            NewSessionConnected += new SessionHandler<ClientSession>(OnClientConnect);
            // 세션 닫힘
            SessionClosed += new SessionHandler<ClientSession, CloseReason>(OnClientDisConnect);
            // 새로운 요청 받음
            NewRequestReceived += new RequestHandler<ClientSession, EFBinaryRequestInfo>(OnPacketReceived);
        }

        public void InitConfig(ServerOption options)
        {
            serverOption = options;

            m_Config = new SuperSocket.SocketBase.Config.ServerConfig
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
                bool isSuccess = Setup(new SuperSocket.SocketBase.Config.RootConfig(), m_Config, logFactory: new NLogLogFactory());

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
                packetProcessor.ProcessorStart(this);

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
        }
        void OnClientDisConnect(ClientSession clientSession, CloseReason reason)
        {
            MainLogger.Info($"Closed Session : {clientSession.SessionID}");

            ServerPacketData packet = new ServerPacketData();
            packet.SetPacketNoBody(clientSession.SessionID, (Int16)PACKET_ID.IN_NTF_CLIENT_DISCONNECT);

            PassToProcessor(packet);
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
    }

    public class ClientSession : AppSession<ClientSession, EFBinaryRequestInfo>
    {

    }
}