namespace PacketDefine
{
    public class ConstDefine
    {
        public const Int16 PACKET_HEADER_SIZE = 4;
        public const int MAX_USER_ID_BYTE_LENGTH = 16;
        public const int MAX_USER_PW_BYTE_LENGTH = 16;
    }
    public class PacketToBytes
    {
        public static byte[] MakeToPacket(PACKET_ID packetId, byte[] bodyData)
        {
            Int16 pktId = (Int16)packetId;
            Int16 bodyDataSize = 0;
            if(bodyData != null)
            {
                bodyDataSize = (Int16)bodyData.Length;
            }

            Int16 packetSize = (Int16)(ConstDefine.PACKET_HEADER_SIZE + bodyDataSize);

            byte[] byteData = new byte[packetSize];
            // 패킷 사이즈(2), 패킷 아이디(2), 바디(크기만큼)
            Buffer.BlockCopy(BitConverter.GetBytes(packetSize), 0, byteData, 0, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(pktId), 0, byteData, 2, 2);
            
            if(bodyData != null)
            {
                Buffer.BlockCopy(bodyData, 0, byteData, 4, bodyDataSize);
            }

            return byteData;
        }

        /*public static Tuple<int, byte[]> ResolveBytes(int recvLength, byte[] recvData)
        {
            Int16 packetSize = BitConverter.ToInt16(recvData, 0);
            Int16 packetId = BitConverter.ToInt16(recvData, 2); // 패킷의 2번째 인덱스부터 ID
            var bodySize = packetSize - ConstDefine.PACKET_HEADER_SIZE;

            byte[] packetBody = new byte[bodySize];
            Buffer.BlockCopy(recvData, ConstDefine.PACKET_HEADER_SIZE, packetBody, 0, bodySize);

            return new Tuple<int, byte[]>(packetId, packetBody);
        }*/
    }
}
