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
        LoginResponse = 2001,
        FullUser = 2002,

        // Room
        RoomEnterRequest = 3000,
        RoomEnterResponse = 3001,
        RoomLeaveRequest = 3002,
        RoomLeaveResponse = 3003,
        RoomEnterNotify = 3004,
        RoomLeaveNotify = 3005,
        RoomMemberNotify = 3006,
        RoomChatRequest = 3007,
        RoomChatResponse = 3008,
        RoomChatNotify = 3009,

        // Game Ready
        GameReadyRequest = 4000,
        GameReadyResponse = 4001,

        // Game Start
        GameStartRequest = 4002,
        GameStartResponse = 4003,
        GameStartNotify = 4004,

        // Omok Game
        OmokStonePlaceRequest = 5000,
        OmokStonePlaceResponse = 5001,
        OmokStonePlaceNotify = 5002,
        OmokWinNotify = 5003,

        // Turn Change
        TurnChangeNotify = 6000,

        // Heart Beat
        HeartBeatRequestToClient = 7000,
        HeartBeatResponseFromClient = 7001,
    }
}
