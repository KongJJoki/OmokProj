namespace PacketDefine
{
    public enum PACKET_ID
    {
        // Internal
        InNTFClientConnect = 1000,
        InNTFClientDisconnect = 1001,



        // External
        // Login
        LoginRequest = 2000,
        LoginRespond = 2001,
        FullUser = 2002,

        // Room
        RoomEnterRequest = 3000,
        RoomEnterRespond = 3001,
        RoomLeaveRequest = 3002,
        RoomLeaveRespond = 3003,
        RoomEnterNotify = 3004,
        RoomLeaveNotify = 3005,
        RoomMemberNotify = 3006,
        RoomChatRequest = 3007,
        RoomChatRespond = 3008,
        RoomChatNotify = 3009,

        // Game Ready
        GameReadyRequest = 4000,
        GameReadyResponse = 4001,
        GameStartNotify = 4002,
    }
}
