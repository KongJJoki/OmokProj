namespace Game_Server
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
    }
}