namespace GameServer.Model.DTO
{
    public class GameLoginReq
    {
        public int Uid { get; set; }
        public string AuthToken { get; set; }
    }
    public class GameLoginRes : ErrorCode
    {
        public string SockIP { get; set; }
        public string SockPort { get; set; }
    }
}