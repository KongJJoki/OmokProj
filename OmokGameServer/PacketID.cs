using MemoryPack;
using System.ComponentModel.DataAnnotations;

namespace PacketDefine
{
    public enum PACKET_ID
    {
        // Internal
        IN_NTF_CLIENT_CONNECT = 1000,
        IN_NTF_CLIENT_DISCONNECT = 1001,



        // External
        LOGIN_REQUEST = 2000,
        LOGIN_RESPOND = 2001,
        FULL_USER = 2002,
    }
}
