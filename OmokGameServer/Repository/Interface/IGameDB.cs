namespace OmokGameServer
{
    public interface IGameDB : IDisposable
    {
        public Task<int> GetWinCount(string userId);
        public Task<int> GetLoseCount(string userId);
        public Task UpdateWinCount(string userId, int originWinCount);
        public Task UpdateLoseCount(string userId, int originLoseCount);
    }
}