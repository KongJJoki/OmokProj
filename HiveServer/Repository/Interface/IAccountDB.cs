using HiveServer.Model.DAO;

namespace HiveServer.Repository
{
    public interface IAccountDB : IDisposable
    {
        public Task<int> InsertAccount(string email, string securePassword);
        public Task<bool> AccountExistCheck(string id);
        public Task<Account> GetAccountInfo(string id);
    }
}