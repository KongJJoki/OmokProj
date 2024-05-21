using static GameServer.Services.GameLoginService;

namespace GameServer.Services.Interface
{
    public interface IGameLoginService
    {
        public Task<GameLoginResult> GameLogin(int userId, string authToken);
    }
}