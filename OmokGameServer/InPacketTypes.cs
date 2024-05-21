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
        public int WinUserUid { get; set; }
        public int LoseUseUid { get; set; }
    }

    [MemoryPackable]
    public partial class InPKTVerifiedLoginReq
    {
        public SockErrorCode ErrorCode { get; set; }
        public int Uid { get; set; }
        public string AuthToken { get; set; }
    }
}