namespace Game_Server.Repository
{
    public interface IRedisDB
    {
        public Task<bool> InsertAuthToken(string userId, string authToken);
    }
}