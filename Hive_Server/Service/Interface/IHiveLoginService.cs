namespace Hive_Server.Services.Interface
{
    public interface IHiveLoginService
    {
        public Task<(EErrorCode, Int32?, string?)> HiveLogin(string email, string password);
    }
}