namespace HiveServer.Repository
{
    public interface IRedisDB
    {
        public Task<bool> InsertAuthToken(string userId, string authToken);
        public Task<string> GetAuthToken(string userId);
    }
}