using Microsoft.AspNetCore.Mvc;
using Hive_Server.Services.Interface;
using Hive_Server.Services;
using Hive_Server.Model.DTO;
using Microsoft.AspNetCore.Http;

namespace Hive_Server.Controller
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