using MemoryPack;
using System;
using System.Collections.Generic;

namespace CSCommon
{
    [MemoryPackable]
    public partial class PKTResponse
    {
        public short Result;
    }

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
        public SockErrorCode Result { get; set; }
    }

    [MemoryPackable]
    public partial class PKTResFullUser
    {
        public SockErrorCode Result { get; set; }
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
        public SockErrorCode Result { get; set; }
    }

    [MemoryPackable]
    public partial class PKTReqRoomLeave
    {
        public int RoomNumber { get; set; }
    }
    [MemoryPackable]
    public partial class PKTResRoomLeave
    {
        public SockErrorCode Result { get; set; }
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
        public SockErrorCode Result { get; set; }
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
        public SockErrorCode Result { get; set; }
    }

    // Game Start
    [MemoryPackable]
    public partial class PKTReqGameStart
    {

    }

    [MemoryPackable]
    public partial class PKTResGameStart
    {
        public SockErrorCode Result { get; set; }
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
        public SockErrorCode Result { get; set; }
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
