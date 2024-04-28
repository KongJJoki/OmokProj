namespace PacketDefine
{
    public enum PACKET_ID
    {
        // Internal
        IN_NTF_CLIENT_CONNECT = 1000,
        IN_NTF_CLIENT_DISCONNECT = 1001,



        // External
        // Login
        LOGIN_REQUEST = 2000,
        LOGIN_RESPOND = 2001,
        FULL_USER = 2002,

        // Room
        ROOM_ENTER_REQUEST = 3000,
        ROOM_ENTER_RESPOND = 3001,
    }
}
