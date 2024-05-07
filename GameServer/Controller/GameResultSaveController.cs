using Microsoft.AspNetCore.Mvc;
using GameServer.Services.Interface;
using GameServer.Services;
using GameServer.Model.DTO;
using Microsoft.AspNetCore.Http;

namespace GameServer.Controller
{
    [Route("[controller]")]
    [ApiController]
    public class GameResultSaveController : ControllerBase
    {
        private IGameResultSaveService gameResultSaveService;
        public GameResultSaveController(IGameResultSaveService gameResultSaveService)
        {
            this.gameResultSaveService = gameResultSaveService;
        }

        [HttpPost]
        public async Task<GameResultSaveRes> GameResultSave(GameReslutSaveReq req)
        {
            GameResultSaveRes res = new GameResultSaveRes();
            res.Result = await gameResultSaveService.GameResultSave(req.WinUserId, req.LoseUserId);
            return res;
        }
    }
}