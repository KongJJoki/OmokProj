using SuperSocket.SocketBase.Protocol;

namespace OmokGameServer
{
    public class OmokBinaryRequestInfo : BinaryRequestInfo
    {
        public int Size { get; private set; }
        public int PacketId { get; private set; }

        public OmokBinaryRequestInfo(int size, int packetId, byte[] body)
            :base(null, body)
        {
            this.Size = size;
            this.PacketId = packetId;
        }
    }
}
