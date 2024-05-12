namespace MatchingServer.Model.DTO
{
    public class CheckMatchReq
    {
        public int Uid { get; set; }
    }

    public class CheckMatchRes : ErrorCode
    {
        //public string ServerAddress { get; set; } = "";
        public int RoomNumber { get; set; } = 0;
    }
}
