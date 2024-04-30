namespace CSCommon
{
    public enum ERROR_CODE
    {
        None = 0,

        // 전송
        SendFailSessionNone = 1000,


        // 로그인
        LoginFailUserCountLimitExceed = 2000,
        LoginFailAlreadyExistSession = 2001,

        RemoveFailNotExistSession = 2002,

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


    }
}
