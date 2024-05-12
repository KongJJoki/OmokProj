namespace GameServer.Repository
{
    public interface IGameDB : IDisposable
    {
        public Task<bool> GetUserDataExist(Int32 uid);
        public Task<int> InsertBasicData(Int32 uid);
    }
}