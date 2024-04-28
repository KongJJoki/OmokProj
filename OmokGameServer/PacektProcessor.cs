using PacketDefine;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace OmokGameServer
{
    class PacketProcessor
    {
        bool isProcessorRunning = false;
        Thread PacketProcessorTh = null;

        BufferBlock<ServerPacketData> PacketBuffer = new BufferBlock<ServerPacketData>();

        // 유저 매니저 선언
        UserManager userManager = new UserManager();

        // 방 번호 범위 튜플
        // 방 객체 리스트

        // 패킷 핸들러 맵 선언
        // 모든 함수들이 ServerPacketData를 매개변수로 받아서 쓰기 때문에 value가 Action<ServerPacketData>
        Dictionary<int, Action<ServerPacketData>> packetHandlerDictionary = new Dictionary<int, Action<ServerPacketData>>();
        // 일반 패킷핸들러 선언
        LoginPacketHandler loginPacketHandler = new LoginPacketHandler();
        // 방 용 패킷 핸들러 선언

        // 패킷 프로세서 시작하는 함수
        public void ProcessorStart(MainServer mainServer)
        {
            userManager.SetMaxUserNumber(MainServer.serverOption.RoomMaxCount * MainServer.serverOption.RoomMaxUserCount);

            SetPacketHandlers(mainServer);

            isProcessorRunning = true;
            PacketProcessorTh = new System.Threading.Thread(this.Process);
            PacketProcessorTh.Start();
        }
        // 패킷 프로세서 끄는 함수
        public void ProcessorStop()
        {
            // 루프 멈춰서 자연스럽게 스레드도 종료
            isProcessorRunning = false;
            PacketBuffer.Complete();
        }

        // 버퍼에 패킷 넣는 함수
        public void InsertPacket(ServerPacketData packetData)
        {
            PacketBuffer.Post(packetData);
        }

        // 각 핸들러 초기화하고 딕셔너리에 이벤트에 따른 함수들 등록하게하는 함수
        void SetPacketHandlers(MainServer mainServer)
        {
            loginPacketHandler.Init(mainServer, userManager);
            loginPacketHandler.SetPacketHandler(packetHandlerDictionary);
        }
        // 버퍼에서 패킷 하나씩 꺼내서 핸들러 써서 함수 기능하도록 하는 함수
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