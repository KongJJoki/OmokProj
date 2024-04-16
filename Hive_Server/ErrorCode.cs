namespace Hive_Server
{
    public enum EErrorCode
    {
        None = 0,

        DBError = 1,
        RedisError = 2,

        AccountCreateFail = 100,
        NotEmailForm = 101,
        AlreadyExistEmail = 102,
        NotExistAccount = 103,
        WrongPassword = 104,

    }
}