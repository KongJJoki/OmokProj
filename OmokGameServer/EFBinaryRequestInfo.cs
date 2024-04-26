using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SuperSocket.Common;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketEngine.Protocol;

namespace OmokGameServer
{
    public class EFBinaryRequestInfo : BinaryRequestInfo
    {
        public Int16 Size { get; private set; }
        public Int16 PacketId { get; private set; }

        public EFBinaryRequestInfo(Int16 size, Int16 packetId, byte[] body)
            :base(null, body)
        {
            this.Size = size;
            this.PacketId = packetId;
        }
    }
}
