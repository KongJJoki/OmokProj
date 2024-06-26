using Microsoft.AspNetCore.Mvc;
using GameServer.Services.Interface;
using GameServer.Services;
using GameServer.Model.DTO;
using Microsoft.AspNetCore.Http;

namespace GameServer.Controller
{
    [Route("[controller]")]
    [ApiController]
    public class GameLoginController : ControllerBase
    {
        private IGameLoginService gameLoginService;
        public GameLoginController(IGameLoginService gameLoginService)
        {
            this.gameLoginService = gameLoginService;
        }

        [HttpPost]
        public async Task<GameLoginRes> GameLogin(GameLoginReq req)
        {
            GameLoginRes res = new GameLoginRes();
            (res.Result, res.SockIP, res.SockPort) = await gameLoginService.GameLogin(req.Uid, req.AuthToken);
            return res;
        }
    }
}