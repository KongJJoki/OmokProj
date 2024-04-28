namespace OmokGameServer
{
    public class RoomManager
    {
        List<Room> roomList = new List<Room>();

        public void CreateRooms()
        {
            int maxRoomCount = MainServer.serverOption.RoomMaxCount;
            int maxUserCount = MainServer.serverOption.RoomMaxUserCount;
            int roomStartNum = MainServer.serverOption.RoomStartNumber;

            int nowLastRoomNum = roomStartNum;

            for(int i = 0; i< maxRoomCount; i++)
            {
                Room room = new Room();
                room.Init(nowLastRoomNum, maxUserCount);

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