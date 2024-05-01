using MemoryPack;
using OmokGameServer;

namespace PacketTypes
{
    // Internal



    // Common

    // Login
    [MemoryPackable]
    public partial class PKTReqLogin
    {
        public string UserId { get; set; }
        public string AuthToken { get; set; }
    }

    [MemoryPackable]
    public partial class PKTResLogin
    {
        public ERROR_CODE Result { get; set; }
    }

    [MemoryPackable]
    public partial class PKTResFullUser
    {
        public ERROR_CODE Result { get; set; }
    }

    // Room
    [MemoryPackable]
    public partial class PKTReqRoomEnter
    {
        public int RoomNumber { get; set; }
    }

    [MemoryPackable]
    public partial class PKTResRoomEnter
    {
        public ERROR_CODE Result { get; set; }
    }

    [MemoryPackable]
    public partial class PKTReqRoomLeave
    {
        public int RoomNumber { get; set; }
    }
    [MemoryPackable]
    public partial class PKTResRoomLeave
    {
        public ERROR_CODE Result { get; set; }
    }

    [MemoryPackable]
    public partial class PKTNTFRoomEnter
    {
        public string UserId { get; set; }
    }
    [MemoryPackable]
    public partial class PKTNTFRoomLeave
    {
        public string UserId { get; set; }
    }
    [MemoryPackable]
    public partial class PKTNTFRoomMember
    {
        public List<string> UserIdList { get; set; } = new List<string>();
    }

    [MemoryPackable]
    public partial class PKTReqRoomChat
    {
        public string Message { get; set; }
    }
    [MemoryPackable]
    public partial class PKTResRoomChat
    {
        public ERROR_CODE Result { get; set; }
    }
    [MemoryPackable]
    public partial class PKTNTFRoomChat
    {
        public string UserId { get; set; }
        public string Message { get; set; }
    }

    // Game Ready
    [MemoryPackable]
    public partial class PKTReqGameReady
    {
        
    }

    [MemoryPackable]
    public partial class PKTResGameReady
    {
        public ERROR_CODE Result { get; set; }
    }

    // Game Start
    [MemoryPackable]
    public partial class PKTReqGameStart
    {

    }

    [MemoryPackable]
    public partial class PKTResGameStart
    {
        public ERROR_CODE Result { get; set; }
    }

    [MemoryPackable]
    public partial class PKTNTFGameStart
    {
        public string StartUserId { get; set; }
    }

    // Omok Game
    [MemoryPackable]
    public partial class PKTReqOmokStonePlace
    {
        public int PosX { get; set; }
        public int PosY { get; set; }
    }

    [MemoryPackable]
    public partial class PKTResOmokStonePlace
    {
        public ERROR_CODE Result { get; set; }
    }

    [MemoryPackable]
    public partial class PKTNTFOmokStonePlace
    {
        public string NextTurnUserId { get; set; }
        public int PosX { get; set; }
        public int PosY { get; set; }
    }

    [MemoryPackable]
    public partial class PKTNTFOmokWin
    {
        public string WinUserId { get; set; }
    }

    [MemoryPackable]
    public partial class PKTNTFOmokLose
    {
        public string LoseUserId { get; set; }
    }

    // Turn Change
    [MemoryPackable]
    public partial class PKTNTFTurnChange
    {
        public string TurnGetUserId { get; set; }
    }

    // Heart Beat
    [MemoryPackable]
    public partial class PKTHeartBeatToClient
    {

    }

    [MemoryPackable]
    public partial class PKTHeartBeatFromClient
    {
        
    }
}