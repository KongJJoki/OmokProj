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
        ROOM_LEAVE_REQUEST = 3002,
        ROOM_LEAVE_RESPOND = 3003,
        ROOM_ENTER_NOTIFY = 3004,
        ROOM_LEAVE_NOTIFY = 3005,
        ROOM_MEMBER_NOTIFY = 3006,
        ROOM_CHAT_REQUEST = 3007,
        ROOM_CHAT_RESPOND = 3008,
        ROOM_CHAT_NOTIFY = 3009,
    }
}
