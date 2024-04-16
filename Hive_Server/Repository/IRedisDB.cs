namespace Hive_Server.Repository
{
    public interface IRedisDB : IDisposable
    {
        public Task<bool> InsertAuthToken(string userId, string authToken);
    }
}