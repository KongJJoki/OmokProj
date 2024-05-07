namespace GameServer.Repository
{
    public interface IGameDB : IDisposable
    {
        public Task<bool> GetUserDataExist(Int32 userId);
        public Task<int> InsertBasicData(Int32 userId);
        public Task<int> GetWinCount(Int32 userId);
        public Task<int> GetLoseCount(Int32 userId);
        public Task<int> UpdateWinCount(Int32 userId, int originWinCount);
        public Task<int> UpdateLoseCount(Int32 userId, int originLoseCount);
    }
}