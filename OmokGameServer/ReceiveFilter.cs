using PacketDefine;
using SuperSocket.Common;
using SuperSocket.SocketEngine.Protocol;

namespace OmokGameServer
{
    public class ReceiveFilter : FixedHeaderReceiveFilter<OmokBinaryRequestInfo>
    {
        public ReceiveFilter() : base(ConstDefine.PACKET_HEADER_SIZE)
        { 

        }

        protected override int GetBodyLengthFromHeader(byte[] header, int offset, int length)
        {
            if(!BitConverter.IsLittleEndian)
            {
                Array.Reverse(header, 0, ConstDefine.PACKET_HEADER_SIZE);
            }

            var packetSize = BitConverter.ToInt16(header, offset);
            var bodySize = packetSize - ConstDefine.PACKET_HEADER_SIZE;

            return bodySize;
        }

        protected override OmokBinaryRequestInfo ResolveRequestInfo(ArraySegment<byte> header, byte[] bodyBuffer, int offset, int length)
        {
            return new OmokBinaryRequestInfo(BitConverter.ToInt16(header.Array, 0),
                                           BitConverter.ToInt16(header.Array, 2),
                                           bodyBuffer.CloneRange(offset, length));
        }
    }
}
