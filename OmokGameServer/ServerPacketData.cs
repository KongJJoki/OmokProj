namespace OmokGameServer
{
    public class ServerPacketData
    {
        public int packetSize;
        public string sessionId;
        public int packetId;
        public byte[] bodyData;

        public void SetPacket(string sessionId, int packetId, byte[] bodyData)
        {
            this.sessionId = sessionId;
            this.packetId = packetId;

            if(bodyData.Length > 0)
            {
                this.bodyData = bodyData;
            }
        }

        public void SetPacketNoBody(string sessionId, int packetId)
        {
            this.sessionId = sessionId;
            this.packetId = packetId;
        }
    }
}