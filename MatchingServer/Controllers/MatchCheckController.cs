using Microsoft.AspNetCore.Mvc;
using MatchingServer.Model.DTO;
using Microsoft.Extensions.Options;

namespace MatchingServer.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class MatchCheckController : Controller
    {
        IMatchWoker _matchWorker;

        public MatchCheckController(IMatchWoker matchWorker)
        {
            _matchWorker = matchWorker;
        }

        [HttpPost]
        public MatchCheckRes MatchCheckRequest(MatchCheckReq request)
        {
            MatchCheckRes response = new MatchCheckRes();

            var completeMatchingData = _matchWorker.GetCompleteMatching(request.Uid);

            if(completeMatchingData != null)
            {
                response.Result = EErrorCode.None;
                response.SockIP = completeMatchingData.IP;
                response.SockPort = completeMatchingData.Port;
                response.RoomNumber = completeMatchingData.RoomNumber;
            }
            else
            {
                response.Result = EErrorCode.NotMatched;
                response.SockIP = "";
                response.SockPort = "";
                response.RoomNumber = 0;
            }

            return response;
        }
    }
}