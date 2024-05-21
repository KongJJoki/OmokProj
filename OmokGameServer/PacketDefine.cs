using GameServerClientShare;

namespace PacketDefine
{
    public class ConstDefine
    {
        public const int PACKET_HEADER_SIZE = 4;
        public const int MAX_USER_ID_BYTE_LENGTH = 16;
        public const int MAX_USER_PW_BYTE_LENGTH = 16;
    }
    public class PacketToBytes
    {
        public static byte[] MakeToPacket(PacketID packetId, byte[] bodyData)
        {
            int pktId = (int)packetId;
            int bodyDataSize = 0;
            if(bodyData != null)
            {
                bodyDataSize = (int)bodyData.Length;
            }

            int packetSize = (int)(ConstDefine.PACKET_HEADER_SIZE + bodyDataSize);

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
    }
}
