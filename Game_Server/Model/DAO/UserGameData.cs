﻿namespace Game_Server.Model.DAO
{
    public class UserGameData
    {
        public Int32 UserId { get; set; }
        public Int32 Level { get; set; }
        public Int32 Exp { get; set; }
        public Int32 WinCount { get; set; }
        public Int32 LoseCount { get; set; }
    }
}
