using Microsoft.AspNetCore.Mvc;
using HiveServer.Services.Interface;
using HiveServer.Services;
using HiveServer.Model.DTO;
using Microsoft.AspNetCore.Http;

namespace HiveServer.Controller
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