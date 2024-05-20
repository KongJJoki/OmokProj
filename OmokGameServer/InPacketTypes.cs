using GameServerClientShare;
using MemoryPack;
using OmokGameServer;

namespace InPacketTypes
{
    // Internal 

    [MemoryPackable]
    public partial class InPKTCheckTurn
    {

    }

    [MemoryPackable]
    public partial class InPKTCheckHeartBeat
    {

    }

    [MemoryPackable]
    public partial class InPKTGameResult
    {
        public Int32 WinUserUid { get; set; }
        public Int32 LoseUseUid { get; set; }
    }

    [MemoryPackable]
    public partial class InPKTVerifiedLoginReq
    {
        public SockErrorCode ErrorCode { get; set; }
        public Int32 Uid { get; set; }
        public string AuthToken { get; set; }
    }
}