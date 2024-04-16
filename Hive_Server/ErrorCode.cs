namespace Hive_Server
{
    public enum EErrorCode
    {
        None = 0,

        DBError = 1,
        RedisError = 2,

        InvalidToken = 10,

        AccountCreateFail = 100,
        NotEmailForm = 101,
        AlreadyExistAccount = 102,

        HiveLoginFail = 110,
        NotExistAccount = 111,
        WrongPassword = 112,

    }
}