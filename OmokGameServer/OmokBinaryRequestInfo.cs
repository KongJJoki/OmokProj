using SuperSocket.SocketBase.Protocol;

namespace OmokGameServer
{
    public class OmokBinaryRequestInfo : BinaryRequestInfo
    {
        public Int16 Size { get; private set; }
        public Int16 PacketId { get; private set; }

        public OmokBinaryRequestInfo(Int16 size, Int16 packetId, byte[] body)
            :base(null, body)
        {
            this.Size = size;
            this.PacketId = packetId;
        }
    }
}
