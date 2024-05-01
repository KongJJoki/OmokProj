namespace OmokGameServer
{
    public class RoomManager
    {
        List<Room> roomList = new List<Room>();
        ServerOption serverOption;

        public void SetServerOption(ServerOption serverOption)
        {
            this.serverOption = serverOption;
        }

        public void CreateRooms()
        {
            int maxRoomCount = serverOption.RoomMaxCount;
            int maxUserCount = serverOption.RoomMaxUserCount;
            int roomStartNum = serverOption.RoomStartNumber;
            int turnTimeLimit = serverOption.TurnTimeLimitSecond;

            int nowLastRoomNum = roomStartNum;

            for(int i = 0; i< maxRoomCount; i++)
            {
                Room room = new Room();
                room.Init(nowLastRoomNum, maxUserCount, turnTimeLimit);

                roomList.Add(room);

                nowLastRoomNum++;
            }
        }
        public List<Room> GetRooms()
        {
            return roomList;
        }
    }
}