namespace OmokGameServer
{
    public class User
    {
        public DateTime lastHeartBeatTime { get; set; }
        public int myUserArrayIndex { get; set; }

        public string sessionId;
        public int uid;
        public int roomNumber = -1;
        public bool isInRoom = false;

        public void SetUser(string sessionId)
        {
            this.sessionId = sessionId;
            lastHeartBeatTime = DateTime.Now;
        }
        
        public void SetUserName(int uid)
        {
            this.uid = uid;
        }

        public void SetRoomEnter(int roomNum)
        {
            isInRoom = true;
            roomNumber = roomNum;
        }
        public void SetRoomLeave()
        {
            isInRoom = false;
            roomNumber = -1;
        }
    }
}