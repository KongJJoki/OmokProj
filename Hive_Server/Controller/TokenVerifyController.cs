using Microsoft.AspNetCore.Mvc;
using Hive_Server.Services.Interface;
using Hive_Server.Services;
using Hive_Server.Model.DTO;
using Microsoft.AspNetCore.Http;

namespace Hive_Server.Controller
{
    [Route("[controller]")]
    [ApiController]
    public class TokenVerifyController : ControllerBase
    {
        private ITokenVerifyService tokenVerifyService;
        public TokenVerifyController(ITokenVerifyService tokenVerifyService)
        {
            this.tokenVerifyService = tokenVerifyService;
        }

        [HttpPost]
        public async Task<TokenVerifyRes> TokenVerify(TokenVerifyReq req)
        {
            TokenVerifyRes res = new TokenVerifyRes();
            res.Result = await tokenVerifyService.TokenVerify(req.UserId.ToString(), req.AuthToken);
            return res;
        }
    }
}