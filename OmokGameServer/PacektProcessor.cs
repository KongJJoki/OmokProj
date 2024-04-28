using System.Threading.Tasks.Dataflow;

namespace OmokGameServer
{
    class PacketProcessor
    {
        bool isProcessorRunning = false;
        Thread PacketProcessorTh = null;

        BufferBlock<ServerPacketData> PacketBuffer = new BufferBlock<ServerPacketData>();

        UserManager userManager;
        RoomManager roomManager;

        // 방 번호 범위 튜플
        // 방 객체 리스트

        // 패킷 핸들러 맵 선언
        // 모든 함수들이 ServerPacketData를 매개변수로 받아서 쓰기 때문에 value가 Action<ServerPacketData>
        Dictionary<int, Action<ServerPacketData>> packetHandlerDictionary = new Dictionary<int, Action<ServerPacketData>>();

        LoginPacketHandler loginPacketHandler = new LoginPacketHandler();
        RoomPacketHandler roomPacketHandler = new RoomPacketHandler();

        public void ProcessorStart(UserManager userManager, RoomManager roomManager)
        {
            this.userManager = userManager;
            this.roomManager = roomManager;

            SetPacketHandlers();

            isProcessorRunning = true;
            PacketProcessorTh = new Thread(Process);
            PacketProcessorTh.Start();
        }

        public void ProcessorStop()
        {
            isProcessorRunning = false;
            PacketBuffer.Complete();
        }

        public void InsertPacket(ServerPacketData packetData)
        {
            PacketBuffer.Post(packetData);
        }

        void SetPacketHandlers()
        {
            loginPacketHandler.Init(userManager);
            loginPacketHandler.SetPacketHandler(packetHandlerDictionary);

            roomPacketHandler.Init(userManager);
            roomPacketHandler.SetRoomList(roomManager.GetRooms());
            roomPacketHandler.SetPacketHandler(packetHandlerDictionary);
        }

        void Process()
        {
            while(isProcessorRunning)
            {
                try
                {
                    ServerPacketData newPacket = PacketBuffer.Receive();

                    if(packetHandlerDictionary.ContainsKey(newPacket.PacketId))
                    {
                        packetHandlerDictionary[newPacket.PacketId](newPacket);
                    }
                    else
                    {
                        MainServer.MainLogger.Debug($"Unknown PacketId : {newPacket.PacketId}"); ;
                    }
                }
                catch(Exception ex)
                {
                    MainServer.MainLogger.Error($"PacketProcessor Error : {ex.ToString()}");
                }
            }
        }

    }
}