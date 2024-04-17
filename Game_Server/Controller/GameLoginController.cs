using Microsoft.AspNetCore.Mvc;
using Game_Server.Services.Interface;
using Game_Server.Services;
using Game_Server.Model.DTO;
using Microsoft.AspNetCore.Http;

namespace Game_Server.Controller
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
            res.Result = await gameLoginService.GameLogin(req.UserId, req.AuthToken);
            return res;
        }
    }
}