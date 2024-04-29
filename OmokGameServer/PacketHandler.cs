using SuperSocket.SocketBase.Logging;

namespace OmokGameServer
{
    public class PacketHandler
    {
        protected UserManager userManager;
        protected RoomManager roomManager;
        protected List<Room> roomList;
        protected ILog mainLogger;
        protected ServerOption serverOption;

        protected Func<string, byte[], bool> sendFunc;
        public void Init(UserManager userManager, RoomManager roomManager, ILog mainLogger, ServerOption serverOption, Func<string, byte[], bool> sendFunc)
        {
            this.userManager = userManager;
            this.roomManager = roomManager;
            roomList = roomManager.GetRooms();
            this.mainLogger = mainLogger;
            this.serverOption = serverOption;
            this.sendFunc = sendFunc;
        }
    }
}