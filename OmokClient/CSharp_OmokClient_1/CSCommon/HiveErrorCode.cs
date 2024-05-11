namespace OmokClient.CSCommon
{
    public enum HiveErrorCode
    {
        None = 0,

        DBError = 1,
        RedisError = 2,

        InvalidToken = 10,
        TokenNotExist = 11,
        TokenVerifyFail = 12,

        AccountCreateFail = 100,
        NotEmailForm = 101,
        AlreadyExistAccount = 102,

        HiveLoginFail = 110,
        NotExistAccount = 111,
        WrongPassword = 112,

        HiveHttpReqFail = 150,
    }
}
