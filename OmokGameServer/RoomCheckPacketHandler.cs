using MemoryPack;
using PacketDefine;
using PacketTypes;
using SuperSocket.SocketBase.Logging;

namespace OmokGameServer
{
    public class RoomCheckPacketHandler : PacketHandler
    {
        TimeSpan turnLimitTime;
        TimeSpan gameDurationLimitTime;
        public int checkRoomNumberOffset;
        public int oneCheckCount;

        public new void Init(UserManager userManager, RoomManager roomManager, ILog mainLogger, ServerOption serverOption, Func<string, byte[], bool> sendFunc)
        {
            base.Init(userManager, roomManager, mainLogger, serverOption, sendFunc);

            turnLimitTime = new TimeSpan(0, 0, 0, serverOption.TurnTimeLimitSecond);
            gameDurationLimitTime = new TimeSpan(serverOption.MaxGameTimeHour, 0, 0);
            checkRoomNumberOffset = 0;
            oneCheckCount = (int)Math.Ceiling((double)serverOption.RoomMaxCount / serverOption.TotalDivideNumber);
        }

        public void SetPacketHandler(Dictionary<int, Action<ServerPacketData>> packetHandlerDictionary)
        {
            packetHandlerDictionary.Add((int)PACKET_ID.InNTFCheckTurnTime, RoomCheckRequest);
        }

        void RoomCheckRequest(ServerPacketData packet)
        {
            for (int i = 0; i < oneCheckCount; i++)
            {
                if (checkRoomNumberOffset == serverOption.RoomMaxCount)
                {
                    break;
                }

                if (!roomList[checkRoomNumberOffset].isGameStart)
                {
                    checkRoomNumberOffset++;
                    continue;
                }

                if (roomManager.CheckGameDuration(checkRoomNumberOffset) >= gameDurationLimitTime)
                {
                    roomList[checkRoomNumberOffset].NotifyGameForceFinish();
                    continue;
                }

                if (roomList[checkRoomNumberOffset].ForceTurnChangeCount >= serverOption.MaxTurnChangeCount)
                {
                    roomList[checkRoomNumberOffset].NotifyGameForceFinish();
                    continue;
                }

                if (roomManager.CheckTurnTimeDiff(checkRoomNumberOffset) >= turnLimitTime)
                {
                    roomList[checkRoomNumberOffset].NotifyTimeOutTurnChange();
                }

                checkRoomNumberOffset++;
            }

            if (checkRoomNumberOffset >= serverOption.RoomMaxCount)
            {
                checkRoomNumberOffset = 0;
            }
        }
    }
}