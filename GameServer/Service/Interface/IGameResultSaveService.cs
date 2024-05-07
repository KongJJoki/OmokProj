namespace GameServer.Services.Interface
{
    public interface IGameResultSaveService
    {
        public Task<EErrorCode> GameResultSave(Int32 winUserId, Int32 loseUserId);
    }
}