namespace OmokGameServer
{
    public class ServerPacketData
    {
        public Int16 packetSize;
        public string sessionId;
        public Int16 packetId;
        public byte[] bodyData;

        public void SetPacket(string sessionId, Int16 packetId, byte[] bodyData)
        {
            this.sessionId = sessionId;
            this.packetId = packetId;

            if(bodyData.Length > 0)
            {
                this.bodyData = bodyData;
            }
        }

        public void SetPacketNoBody(string sessionId, Int16 packetId)
        {
            this.sessionId = sessionId;
            this.packetId = packetId;
        }
    }
}