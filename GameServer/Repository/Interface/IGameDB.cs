namespace GameServer.Repository
{
    public interface IGameDB : IDisposable
    {
        public Task<bool> GetUserDataExist(int uid);
        public Task<int> InsertBasicData(int uid);
    }
}