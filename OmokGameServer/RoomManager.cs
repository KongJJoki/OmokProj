using PacketDefine;
using System.Collections.Concurrent;
using SockInternalPacket;

namespace OmokGameServer
{
    public class RoomManager
    {
        ServerOption serverOption;
        Action<ServerPacketData> pushPacketInProcessorFunc;
        Func<string, bool> userConnectionCloseFunc;

        Timer turnCheckTimer;
        List<Room> roomList = new List<Room>();
        Dictionary<int, int> nowRoomMemberCount = new Dictionary<int, int>(); // 현재 방의 인원 수 -> 1분 이상 1명인 방 있으면 매칭 상대 접속 실패?로 퇴장시키기

        ConcurrentQueue<int> emptyRoomNumber = new ConcurrentQueue<int>();

        public void Init(ServerOption serverOption, Action<ServerPacketData> pushPacketInProcessorFunc, Func<string, bool> userConnectionCloseFunc)
        {
            this.serverOption = serverOption;
            this.pushPacketInProcessorFunc = pushPacketInProcessorFunc;
            this.userConnectionCloseFunc = userConnectionCloseFunc;
            SetTimer();
        }

        public void SetTimer()
        {
            turnCheckTimer = new Timer(SendTurnCheckPacket, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(1000/serverOption.TotalDivideNumber));
        }

        void SendTurnCheckPacket(object state)
        {
            ServerPacketData packet = new ServerPacketData();
            packet.SetPacketNoBody("", (Int16)InPACKET_ID.InNTFCheckTurnTime);

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
                room.Init(nowLastRoomNum, maxUserCount, pushPacketInProcessorFunc, userConnectionCloseFunc);

                roomList.Add(room);
                emptyRoomNumber.Enqueue(nowLastRoomNum);

                nowLastRoomNum++;
            }
        }

        public Room GetRoom(int roomNum)
        {
            int roomIndex = roomNum - serverOption.RoomStartNumber;

            if (roomIndex < 0 || roomIndex > serverOption.RoomMaxCount)
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

        public TimeSpan CheckGameDuration(int roomIndex)
        {
            return DateTime.Now - roomList[roomIndex].gameStartTime;
        }

        public bool IsEmptyRoomExist()
        {
            return !emptyRoomNumber.IsEmpty;
        }

        public int GetEmptyRoomNum()
        {
            int emptyRoomNum = 0;
            emptyRoomNumber.TryDequeue(out emptyRoomNum);
            return emptyRoomNum;
        }

        public void EnqueueEmptyRoom(int roomNum)
        {
            emptyRoomNumber.Enqueue(roomNum);
        }
    }
}