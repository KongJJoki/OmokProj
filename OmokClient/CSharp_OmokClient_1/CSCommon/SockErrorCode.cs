namespace CSCommon
{
    public enum SockErrorCode
    {
        None = 0,

        // 전송
        SendFailSessionNone = 1000,

        // 접속
        ConnectFailUserCountLimitExceed = 1500,
        DisconnectFailUserNotExist = 1501,

        // 로그인
        LoginFailInvalidUser = 2000,
        LoginFailNotConnected = 2001,
        LoginFailUserCountLimitExceed = 2002,
        LoginFailAlreadyExistUser = 2003,

        RemoveFailNotExistSession = 2004,

        // 방 입장, 퇴장
        RoomEnterFailInvalidUser = 3000,
        RoomEnterFailAlreadyInRoom = 3001,
        RoomEnterFailNotExistRoom = 3002,
        RoomEnterFailUserCountLimitExceed = 3003,

        RoomLeaveFailInvalidUser = 3004,
        RoomLeaveFailNotInRoom = 3005,

        RoomChatFailInvalidUser = 3006,
        RoomChatFailNotInRoom = 3007,

        // 게임 준비
        GameReadyFailInvalidUser = 4000,
        GameReadyFailNotInRoom = 4001,
        GameReadyFailAlreadyGameStart = 4002,
        GameReadyFailAlreadyReadyStatus = 4003,

        // 게임 시작
        GameStartFailInvalidUser = 4004,
        GameStartFailNotInRoom = 4005,
        GameStartFailNotEnoughUserCount = 4006,
        GameStartFailAlreadyGameStart = 4007,
        GameStartFailNotReady = 4008,
        GameStartFailNotAllReady = 4009,

        // 오목 게임
        OmokStonePlaceFailInvalidUser = 5000,
        OmokStonePlaceFailGameNotStart = 5001,
        OmokStonePlaceFailAlreadyStoneExist = 5002,

        // 게임 결과 저장
        GameResultSaveFailDBError = 6000,
    }
}
