namespace OmokGameServer
{
    public class User
    {
        public string sessionId;
        public string userId;
        public int roomNumber = -1;
        public bool isInRoom = false;

        public void SetUser(string sessionId, string userId)
        {
            this.sessionId = sessionId;
            this.userId = userId;
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