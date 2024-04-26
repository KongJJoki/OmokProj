namespace OmokGameServer
{
    public class ServerPacketData
    {
        public Int16 PacketSize;
        public string SessionId;
        public Int16 PacketId;
        public byte[] BodyData;

        public void SetPacket(string sessionId, Int16 packetId, byte[] bodyData)
        {
            SessionId = sessionId;
            PacketId = PacketId;

            if(bodyData.Length > 0)
            {
                BodyData = bodyData;
            }
        }
    }
}