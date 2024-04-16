namespace Hive_Server.Repository
{
    public interface IAccountDB : IDisposable
    {
        public Task<int> InsertAccount(string email, string securePassword);
        public Task<bool> AccountExistCheck(string email);
    }
}