namespace HiveServer.Model.DTO
{
    public class TokenVerifyReq
    {
        public Int32 Uid { get; set; }
        public string AuthToken { get; set; }
    }
    public class TokenVerifyRes : ErrorCode
    {
        
    }
}