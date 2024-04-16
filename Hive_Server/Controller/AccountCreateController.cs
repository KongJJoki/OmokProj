using Microsoft.AspNetCore.Mvc;
using Hive_Server.Services.Interface;
using Hive_Server.Services;
using Hive_Server.Model.DTO;
using Microsoft.AspNetCore.Http;

namespace Hive_Server.Controller
{
    [Route("[controller]")]
    [ApiController]
    public class AccountCreateController : ControllerBase
    {
        private IAccountCreateService accountCreateService;
        public AccountCreateController(IAccountCreateService accountCreateService)
        {
            this.accountCreateService = accountCreateService;
        }

        [HttpPost]
        public async Task<AccountCreateRes> AccountCreate(AccountCreateReq req)
        {
            AccountCreateRes res = new AccountCreateRes();
            res.Result = await accountCreateService.AccountCreate(req.Email, req.Password);
            return res;
        }
    }
}