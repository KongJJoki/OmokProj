using PacketDefine;

namespace OmokGameServer
{
    public class RoomManager
    {
        ServerOption serverOption;
        Action<ServerPacketData> pushPacketInProcessorFunc;

        Timer turnCheckTimer;
        List<Room> roomList = new List<Room>();

        public void Init(ServerOption serverOption, Action<ServerPacketData> pushPacketInProcessorFunc)
        {
            this.serverOption = serverOption;
            this.pushPacketInProcessorFunc = pushPacketInProcessorFunc;
            SetTimer();
        }

        public void SetTimer()
        {
            turnCheckTimer = new Timer(SendTurnCheckPacket, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(1000/serverOption.TotalDivideNumber));
        }

        void SendTurnCheckPacket(object state)
        {
            ServerPacketData packet = new ServerPacketData();
            packet.SetPacketNoBody("", (Int16)PACKET_ID.InNTFCheckTurnTime);

            pushPacketInProcessorFunc(packet);
        }

        public void CreateRooms()
        {
            int maxRoomCount = serverOption.RoomMaxCount;
            int maxUserCount = serverOption.RoomMaxUserCount;
            int roomStartNum = serverOption.RoomStartNumber;

            int nowLastRoomNum = roomStartNum;

            for(int i = 0; i< maxRoomCount; i++)
            {
                Room room = new Room();
                room.Init(nowLastRoomNum, maxUserCount);

                roomList.Add(room);

                nowLastRoomNum++;
            }
        }

        public Room GetRoom(int roomNum)
        {
            int roomIndex = roomNum - serverOption.RoomStartNumber;

            if (roomIndex < serverOption.RoomStartNumber - 1 || roomIndex > serverOption.RoomMaxCount)
            {
                return null;
            }

            return roomList[roomIndex];
        }

        public List<Room> GetRooms()
        {
            return roomList;
        }

        public TimeSpan CheckTurnTimeDiff(int roomIndex)
        {
            return DateTime.Now - roomList[roomIndex].lastStonePlaceTime;
        }
    }
}