namespace Game_Server.Model.DTO
{
    public class GameLoginReq
    {
        public Int32 UserId { get; set; }
        public string AuthToken { get; set; }
    }
    public class GameLoginRes : ErrorCode
    {
        
    }
}