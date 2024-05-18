namespace OmokGameServer
{
    public class ServerOption
    {
        public int ServerUniqueID { get; set; }

        public int IP { get; set; }

        public int Port { get; set; }

        public string Name { get; set; }

        public int MaxConnectionNumber { get; set; }

        public int MaxRequestLength { get; set; }

        public int ReceiveBufferSize { get; set; }

        public int SendBufferSize { get; set; }

        public int RoomMaxCount { get; set; } = 0;

        public int RoomMaxUserCount { get; set; } = 0;

        public int RoomStartNumber { get; set; } = 0;

        public int TurnTimeLimitSecond { get; set; }

        public int HeartBeatTimeLimitSecond { get; set; }

        public int TotalDivideNumber { get; set; }

        public int MaxTurnChangeCount { get; set; }

        public int MaxGameTimeHour { get; set; }

        public int DBProcessThreadCount { get; set; }
        public int RedisProcessThreadCount { get; set; }
    }
}