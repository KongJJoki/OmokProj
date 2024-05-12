using Microsoft.AspNetCore.Mvc;
using GameServer.Services.Interface;
using GameServer.Services;
using GameServer.Model.DTO;
using Microsoft.AspNetCore.Http;

namespace GameServer.Controller
{
    [Route("[controller]")]
    [ApiController]
    public class MatchRequestController : ControllerBase
    {
        private IMatchRequestService matchRequestService;
        public MatchRequestController(IMatchRequestService matchRequestService)
        {
            this.matchRequestService = matchRequestService;
        }

        [HttpPost]
        public async Task<MatchRequestRes> MatchRequest(MatchRequest req)
        {
            MatchRequestRes res = new MatchRequestRes();
            res.Result = await matchRequestService.MatchRequest(req.Uid);
            return res;
        }
    }
}