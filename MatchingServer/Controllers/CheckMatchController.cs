using Microsoft.AspNetCore.Mvc;
using MatchingServer.Model.DTO;

namespace MatchingServer.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class CheckMatchController : Controller
    {
        IMatchWoker _matchWorker;


        public CheckMatchController(IMatchWoker matchWorker)
        {
            _matchWorker = matchWorker;
        }

        [HttpPost]
        public CheckMatchRes CheckMatchRequest(CheckMatchReq request)
        {
            CheckMatchRes response = new();

            (var result, var completeMatchingData) = _matchWorker.GetCompleteMatching(request.Uid);

            //TODO: 결과를 담아서 보낸다
            if(result)
            {
                response.Result = EErrorCode.None;
                response.RoomNumber = completeMatchingData.RoomNumber;
            }
            else
            {
                response.Result = EErrorCode.NotMatched;
                response.RoomNumber = 0;
            }

            return response;
        }
    }
}