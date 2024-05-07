namespace GameServer.Model.DTO
{
    public class GameReslutSaveReq
    {
        public Int32 WinUserId { get; set; }
        public Int32 LoseUserId { get; set; }
    }
    public class GameResultSaveRes : ErrorCode
    {
        
    }
}