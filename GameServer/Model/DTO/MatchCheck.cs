namespace GameServer.Model.DTO
{
    public class MatchCheckReq
    {
        public int Uid { get; set; }
    }
    public class MatchCheckRes : ErrorCode
    {
        public string SockIP { get; set; } = "";
        public string SockPort { get; set; } = "";
        public int RoomNum { get; set; } = 0;
    }
}