using Microsoft.AspNetCore.Mvc;
using GameServer.Services.Interface;
using GameServer.Services;
using GameServer.Model.DTO;
using Microsoft.AspNetCore.Http;

namespace GameServer.Controller
{
    [Route("[controller]")]
    [ApiController]
    public class MatchCheckController : ControllerBase
    {
        private IMatchCheckService matchCheckService;
        public MatchCheckController(IMatchCheckService matchCheckService)
        {
            this.matchCheckService = matchCheckService;
        }

        [HttpPost]
        public async Task<MatchCheckRes> MatchCheck()
        {
            MatchCheckReq req = new MatchCheckReq();
            HttpContext.Request.Headers.TryGetValue("Uid", out var uid);
            req.Uid = Convert.ToInt32(uid.FirstOrDefault());

            MatchCheckRes res = new MatchCheckRes();
            res = await matchCheckService.MatchCheck(req.Uid);
            return res;
        }
    }
}