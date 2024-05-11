namespace GameServer.Repository
{
    public interface IGameDB : IDisposable
    {
        public Task<bool> GetUserDataExist(Int32 userId);
        public Task<int> InsertBasicData(Int32 userId);
    }
}