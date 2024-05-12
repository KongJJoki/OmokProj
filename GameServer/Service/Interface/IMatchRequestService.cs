using static GameServer.Services.GameLoginService;

namespace GameServer.Services.Interface
{
    public interface IMatchRequestService
    {
        public Task<EErrorCode> MatchRequest(int uid);
    }
}