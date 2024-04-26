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

        ServerOption serverOption;
        IServerConfig m_Config;

        // 패킷 프로세서 선언
        
        // 룸 매니저 선언

        public MainServer(IOptions<ServerOption> serverConfig)
            :base(new DefaultReceiveFilterFactory<ReceiveFilter, EFBinaryRequestInfo>)
        {
            serverOption = serverConfig.Value;

            // 새로운 세션 연결
            // 세션 닫힘
            // 새로운 요청 받음
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

                MainLogger.Info("서버 생성 성공");
            }
            catch(Exception ex)
            {
                Console.WriteLine($"[Error] 서버 생성 실패 : {ex}")
            }
        }
        public void StopServer()
        {
            Stop();

            // 패킷 프로세서 삭제
        }
    }

    public class ClientSession : AppSession<ClientSession, EFBinaryRequestInfo>
    {

    }
}