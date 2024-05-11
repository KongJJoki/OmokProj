namespace OmokClient.CSCommon
{
    public enum ApiErrorCode
    {
        None = 0,

        DBError = 1,
        RedisError = 2,

        InvalidToken = 10,
        TokenNotExist = 11,
        TokenVerifyFail = 12,
        HttpReqFail = 13,
        GameApiLoginFail = 14,

        GameApiHttpReqFail = 20,
    }
}
