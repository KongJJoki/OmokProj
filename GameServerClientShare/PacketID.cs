namespace GameServerClientShare
{
    public enum PacketID
    {
        // External

        //From Client 2000~5000
        FromClientStart = 2000,

        // Login
        LoginRequest = 2100,

        // Room
        RoomEnterRequest = 2200,
        RoomLeaveRequest = 2201,
        RoomChatRequest = 2202,

        // Game Ready
        GameReadyRequest = 2300,

        // Game Start
        GameStartRequest = 2401,

        // Omok Game
        OmokStonePlaceRequest = 2500,

        // Heart Beat
        HeartBeatResponseFromClient = 2600,

        FromClientEnd = 5000,



        // To Client 6000~
        // Force DisConnect
        ForceDisconnect = 6000,

        // Login
        LoginResponse = 6100,
        FullUser = 6102,

        // Room
        RoomEnterResponse = 6200,
        RoomLeaveResponse = 6201,
        RoomEnterNotify = 6203,
        RoomLeaveNotify = 6204,
        RoomMemberNotify = 6205,
        RoomChatResponse = 6206,
        RoomChatNotify = 6207,

        // Game Ready
        GameReadyResponse = 6300,

        // Game Start
        GameStartResponse = 6400,
        GameStartNotify = 6401,

        // Omok Game
        OmokStonePlaceResponse = 6500,
        OmokStonePlaceNotify = 6501,
        OmokWinNotify = 6502,
        OmokLoseNotify = 6503,

        // Turn Change
        TurnChangeNotify = 6600,
        OmokForceFinish = 6601,

        // Heart Beat
        HeartBeatRequestToClient = 6700,
    }
}
