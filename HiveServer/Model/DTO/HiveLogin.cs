namespace HiveServer.Model.DTO
{
    public class HiveLoginReq
    {
        public string Id { get; set; }
        public string Password { get; set; }
    }
    public class HiveLoginRes : ErrorCode
    {
        public int Uid { get; set; }
        public string AuthToken { get; set; }
    }
}