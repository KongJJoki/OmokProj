using Microsoft.AspNetCore.Mvc;
using HiveServer.Services.Interface;
using HiveServer.Services;
using HiveServer.Model.DTO;
using Microsoft.AspNetCore.Http;

namespace HiveServer.Controller
{
    [Route("[controller]")]
    [ApiController]
    public class HiveLoginController : ControllerBase
    {
        private IHiveLoginService hiveLoginService;
        public HiveLoginController(IHiveLoginService hiveLoginService)
        {
            this.hiveLoginService = hiveLoginService;
        }

        [HttpPost]
        public async Task<HiveLoginRes> HiveLogin(HiveLoginReq req)
        {
            HiveLoginRes res = new HiveLoginRes();
            (res.Result, res.UserId, res.AuthToken) = await hiveLoginService.HiveLogin(req.Email, req.Password);
            return res;
        }
    }
}