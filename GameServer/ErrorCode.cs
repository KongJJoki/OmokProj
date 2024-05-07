namespace GameServer
{
    public enum EErrorCode
    {
        None = 0,

        DBError = 1,
        RedisError = 2,

        InvalidToken = 10,
        TokenNotExist = 11,
        TokenVerifyFail = 12,
        HttpReqFail = 13,
        GameLoginFail = 14,
        GameResultSaveFail = 15,
        GameWinResultSaveFail = 16,
        GameLoseResultSaveFail = 17
    }
}