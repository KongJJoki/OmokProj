using PacketDefine;
using SuperSocket.Common;
using SuperSocket.SocketEngine.Protocol;

namespace OmokGameServer
{
    public class ReceiveFilter : FixedHeaderReceiveFilter<EFBinaryRequestInfo>
    {
        public ReceiveFilter() : base(PacketDefine.ConstDefine.PACKET_HEADER_SIZE)
        { 

        }

        protected override int GetBodyLengthFromHeader(byte[] header, int offset, int length)
        {
            if(!BitConverter.IsLittleEndian)
            {
                Array.Reverse(header, 0, PacketDefine.ConstDefine.PACKET_HEADER_SIZE);
            }

            Int16 packetSize = BitConverter.ToInt16(header, offset);
            var bodySize = packetSize - PacketDefine.ConstDefine.PACKET_HEADER_SIZE;

            return bodySize;
        }

        protected override EFBinaryRequestInfo ResolveRequestInfo(ArraySegment<byte> header, byte[] bodyBuffer, int offset, int length)
        {
            if(!BitConverter.IsLittleEndian)
            {
                Array.Reverse(header.Array, 0, PacketDefine.ConstDefine.PACKET_HEADER_SIZE);
            }

            return new EFBinaryRequestInfo(BitConverter.ToInt16(header.Array, 0),
                                           BitConverter.ToInt16(header.Array, 2),
                                           bodyBuffer.CloneRange(offset, length));
        }
    }
}
