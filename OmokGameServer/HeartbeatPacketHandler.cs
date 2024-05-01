using Antlr4.Runtime.Tree;
using MemoryPack;
using PacketDefine;
using PacketTypes;

namespace OmokGameServer
{
    public class HeartbeatPacketHandler : PacketHandler
    {
        Timer heartbeatPacketTimer;
        public event EventHandler<HeartbeatEventArgs> HeartbeatPacketReceived;

        void OnHeartbeatPacketReceived(HeartbeatEventArgs eventArgs)
        {
            HeartbeatPacketReceived.Invoke(this, eventArgs);
        }
        public void SetPacketHandler(Dictionary<int, Action<ServerPacketData>> packetHandlerDictionary)
        {
            heartbeatPacketTimer = new Timer(HeartbeatRequestToClient, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
            packetHandlerDictionary.Add((int)PACKET_ID.HeartBeatResponseFromClient, HeartbeatResponseFromClient);
        }

        public void HeartbeatRequestToClient(object state)
        {
            PKTHeartBeatToClient heartbeatToClient = new PKTHeartBeatToClient();

            var bodyData = MemoryPackSerializer.Serialize(heartbeatToClient);
            var sendData = PacketToBytes.MakeBytes(PACKET_ID.HeartBeatRequestToClient, bodyData);

            foreach(var user in userManager.GetNowConnectUsers())
            {
                sendFunc(user.sessionId, sendData);
            }    
        }

        public void HeartbeatResponseFromClient(ServerPacketData packet)
        {
            User user = userManager.GetUser(packet.sessionId);
            OnHeartbeatPacketReceived(new HeartbeatEventArgs(user));
        }
    }
    public class HeartbeatEventArgs : EventArgs
    {
        public User User { get; }
        public HeartbeatEventArgs(User user)
        {
            User = user;
        }
    }
}