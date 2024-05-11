namespace HiveServer.Model.DTO
{
    public class AccountCreateReq
    {
        public string Id { get; set; }
        public string Password { get; set; }
    }
    public class AccountCreateRes : ErrorCode
    {
        
    }
}