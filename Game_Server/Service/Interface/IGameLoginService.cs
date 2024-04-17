namespace Game_Server.Services.Interface
{
    public interface IGameLoginService
    {
        public Task<EErrorCode> GameLogin(Int32 userId, string authToken);
    }
}