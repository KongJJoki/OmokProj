using GameServer.Model.DTO;

namespace GameServer.Services.Interface
{
    public interface IMatchCheckService
    {
        public Task<MatchCheckRes> MatchCheck(int uid);
    }
}