using MemoryPack;
using OmokGameServer;

namespace PacketTypes
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
        public ERROR_CODE ErrorCode { get; set; }
        public Int32 Uid { get; set; }
        public string AuthToken { get; set; }
    }


    // Common

    // Force Disconnect
    [MemoryPackable]
    public partial class PKTNTFForceDisconnect
    {

    }

    // Login
    [MemoryPackable]
    public partial class PKTReqLogin
    {
        public Int32 Uid { get; set; }
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
        public Int32 Uid { get; set; }
    }
    [MemoryPackable]
    public partial class PKTNTFRoomLeave
    {
        public Int32 Uid { get; set; }
    }
    [MemoryPackable]
    public partial class PKTNTFRoomMember
    {
        public List<Int32> UidList { get; set; } = new List<Int32>();
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
        public Int32 Uid { get; set; }
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
        public Int32 StartUserUid { get; set; }
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
        public Int32 NextTurnUserUid { get; set; }
        public int PosX { get; set; }
        public int PosY { get; set; }
    }

    [MemoryPackable]
    public partial class PKTNTFOmokWin
    {
        public Int32 WinUserUid { get; set; }
    }

    [MemoryPackable]
    public partial class PKTNTFOmokLose
    {
        public Int32 LoseUserUid { get; set; }
    }

    // Turn Change
    [MemoryPackable]
    public partial class PKTNTFTurnChange
    {
        public Int32 TurnGetUserUid { get; set; }
    }

    [MemoryPackable]
    public partial class PKTNTFForceGameFinish
    {
        
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