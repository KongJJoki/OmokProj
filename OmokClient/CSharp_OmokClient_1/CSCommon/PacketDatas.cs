﻿using MemoryPack;
using System.Collections.Generic;

namespace CSCommon
{
    [MemoryPackable]
    public partial class PKTResponse
    {
        public short Result;
    }



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
        public int Result { get; set; }
    }

    [MemoryPackable]
    public partial class PKTResFullUser
    {
        public int Result { get; set; }
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
        public int Result { get; set; }
    }

    [MemoryPackable]
    public partial class PKTReqRoomLeave
    {
        public int RoomNumber { get; set; }
    }
    [MemoryPackable]
    public partial class PKTResRoomLeave
    {
        public int Result { get; set; }
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
        public int Result { get; set; }
    }
    [MemoryPackable]
    public partial class PKTNTFRoomChat
    {
        public string UserId { get; set; }
        public string Message { get; set; }
    }
}