using Microsoft.AspNetCore.Mvc;
using MatchingServer.Model.DTO;

namespace MatchingServer.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class MatchRequestController : ControllerBase
    {
        IMatchWoker _matchWorker;

        public MatchRequestController(IMatchWoker matchWorker)
        {
            _matchWorker = matchWorker;
        }

        [HttpPost]
        public MatchResponse MatchRequest(MatchRequest request)
        {
            MatchResponse response = new MatchResponse();

            _matchWorker.AddUser(request.Uid);

            return response;
        }
    }
}