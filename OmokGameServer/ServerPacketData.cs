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
            PacketId = packetId;

            if(bodyData.Length > 0)
            {
                BodyData = bodyData;
            }
        }

        public void SetPacketNoBody(string sessionId, Int16 packetId)
        {
            SessionId = sessionId;
            PacketId = packetId;
        }
    }
}