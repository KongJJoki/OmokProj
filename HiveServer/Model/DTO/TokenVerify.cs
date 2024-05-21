namespace HiveServer.Model.DTO
{
    public class TokenVerifyReq
    {
        public int Uid { get; set; }
        public string AuthToken { get; set; }
    }
    public class TokenVerifyRes : ErrorCode
    {
        
    }
}