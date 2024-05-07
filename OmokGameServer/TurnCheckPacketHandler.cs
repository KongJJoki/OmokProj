using MemoryPack;
using PacketDefine;
using PacketTypes;
using SuperSocket.SocketBase.Logging;

namespace OmokGameServer
{
    public class TurnCheckPacketHandler : PacketHandler
    {
        TimeSpan turnLimitTime;
        public int turnCheckRoomNumberOffset;
        public int oneCheckCount;

        public new void Init(UserManager userManager, RoomManager roomManager, ILog mainLogger, ServerOption serverOption, Func<string, byte[], bool> sendFunc)
        {
            base.Init(userManager, roomManager, mainLogger, serverOption, sendFunc);

            turnLimitTime = new TimeSpan(0, 0, 0, serverOption.TurnTimeLimitSecond);
            turnCheckRoomNumberOffset = 0;
            oneCheckCount = (int)Math.Ceiling((double)serverOption.RoomMaxCount / serverOption.TotalDivideNumber);
        }

        public void SetPacketHandler(Dictionary<int, Action<ServerPacketData>> packetHandlerDictionary)
        {
            packetHandlerDictionary.Add((int)PACKET_ID.InNTFCheckTurnTime, TurnCheckRequest);
        }

        public void TurnCheckRequest(ServerPacketData packet)
        {
            for(int i = 0; i < oneCheckCount; i++)
            {
                if(turnCheckRoomNumberOffset == serverOption.RoomMaxCount)
                {
                    break;
                }

                if (!roomList[turnCheckRoomNumberOffset].isGameStart)
                {
                    turnCheckRoomNumberOffset++;
                    continue;
                }

                if (roomList[turnCheckRoomNumberOffset].ForceTurnChangeCount >= serverOption.MaxTurnChangeCount )
                {
                    roomList[turnCheckRoomNumberOffset].NotifyGameForceFinish();
                    continue;
                }

                TimeSpan timeDiff = roomManager.CheckTurnTimeDiff(turnCheckRoomNumberOffset);
                if(timeDiff > turnLimitTime)
                {
                    roomList[turnCheckRoomNumberOffset].TimeOutTurnChangeNotify();
                }

                turnCheckRoomNumberOffset++;
            }

            if(turnCheckRoomNumberOffset >= serverOption.RoomMaxCount)
            {
                turnCheckRoomNumberOffset = 0;
            }
        }
    }
}