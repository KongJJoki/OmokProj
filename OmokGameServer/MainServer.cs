using SuperSocket.SocketBase;

using PacketDefine;
using SuperSocket.SocketBase.Protocol;

namespace OmokGameServer
{
    public class MainServer : AppServer<ClientSession, EFBinaryRequestInfo>
    {
        public static ServerOption serverOption;
        public static SuperSocket.SocketBase.Logging.ILog MainLogger;

        SuperSocket.SocketBase.Config.IServerConfig m_Config;

        // ��Ŷ ���μ��� ����
        
        // �� �Ŵ��� ����

        public MainServer()
            :base(new DefaultReceiveFilterFactory<ReceiveFilter, EFBinaryRequestInfo>)
        {
            // ���ο� ���� ����
            // ���� ����
            // ���ο� ��û ����
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

            }
            catch
            {

            }
        }
    }

    public class ClientSession : AppSession<ClientSession, EFBinaryRequestInfo>
    {

    }
}