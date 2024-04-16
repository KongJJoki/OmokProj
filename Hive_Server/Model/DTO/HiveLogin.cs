namespace Hive_Server.Model.DTO
{
    public class HiveLoginReq
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
    public class HiveLoginRes : ErrorCode
    {
        public Int32? UserId { get; set; }
        public string? AuthToken { get; set; }
    }
}