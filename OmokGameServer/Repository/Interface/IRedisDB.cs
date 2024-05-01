namespace OmokGameServer
{
    public interface IRedisDB
    {
        public Task<bool> InsertAuthToken(string userId, string authToken);
    }
}