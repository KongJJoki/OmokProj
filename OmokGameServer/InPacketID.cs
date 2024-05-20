namespace SockInternalPacket
{
    public enum InPACKET_ID
    {
        // Internal 1000~1999
        InNTFClientConnect = 1000,
        InNTFClientDisconnect = 1001,
        InNTFCheckTurnTime = 1002,
        InNTFCheckHeartBeat = 1003,
        InSaveGameResult = 1004,
        InVerifiedLoginRequest = 1005
    }
}