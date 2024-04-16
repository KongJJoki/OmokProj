namespace Hive_Server.Model.DTO
{
    public class AccountCreateReq
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
    public class AccountCreateRes : ErrorCode
    {
        
    }
}