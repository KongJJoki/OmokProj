using MemoryPack;

namespace PacketTypes
{
    // Internal




    // Common
    [MemoryPackable]
    public partial class PKTReqLogin
    {
        public string UserId { get; set; }
        public string AuthToken { get; set; }
    }

    [MemoryPackable]
    public partial class PKTResLogin
    {
        public int Result { get; set; }
    }

    [MemoryPackable]
    public partial class PKTResFullUser
    {
        public int Result { get; set; }
    }
}