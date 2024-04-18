namespace Hive_Server.Services.Interface
{
    public interface IHiveLoginService
    {
        public Task<HiveLoginService.HiveLoginResult> HiveLogin(string email, string password);
    }
}