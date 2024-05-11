using static GameServer.Services.GameLoginService;

namespace GameServer.Services.Interface
{
    public interface IGameLoginService
    {
        public Task<GameLoginResult> GameLogin(Int32 userId, string authToken);
    }
}