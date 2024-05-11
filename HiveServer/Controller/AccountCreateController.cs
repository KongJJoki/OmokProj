using Microsoft.AspNetCore.Mvc;
using HiveServer.Services.Interface;
using HiveServer.Services;
using HiveServer.Model.DTO;
using Microsoft.AspNetCore.Http;

namespace HiveServer.Controller
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
            res.Result = await accountCreateService.AccountCreate(req.Id, req.Password);
            return res;
        }
    }
}