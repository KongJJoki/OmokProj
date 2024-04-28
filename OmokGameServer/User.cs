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
        public void SetRoomNumber(int roomNum)
        {
            isInRoom = true;
            roomNumber = roomNum;
        }
    }
}