namespace Hive_Server.Model.DTO
{
    public class TokenVerifyReq
    {
        public Int32 UserId { get; set; }
        public string AuthToken { get; set; }
    }
    public class TokenVerifyRes : ErrorCode
    {
        
    }
}