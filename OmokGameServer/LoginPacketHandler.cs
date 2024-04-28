using PacketDefine;

namespace OmokGameServer
{
    public class LoginPacketHandler : PacketHandler
    {
        // 핸들러 맵에 함수 추가하는 함수
        public void SetPacketHandler(Dictionary<int, Action<ServerPacketData>> packetHandlerDictionary)
        {
            packetHandlerDictionary.Add((int)PACKET_ID.IN_NTF_CLIENT_CONNECT, InternalNTFClientConnect);
            packetHandlerDictionary.Add((int)PACKET_ID.IN_NTF_CLIENT_DISCONNECT, InterNTFClientDisconnect);
        }
        // 클라이언트 연결
        public void InternalNTFClientConnect(ServerPacketData packetData)
        {
            MainServer.MainLogger.Debug($"New Client Connect / Now Session Count : {mainServer.SessionCount}");
        }
        // 클라이언트 연결 종료
        public void InterNTFClientDisconnect(ServerPacketData packetData)
        {
            // 패킷 데이터에서 유저 정보 가져오기
            string userSessionId = packetData.SessionId;

            // 유저 매니저에서 유저 삭제 요청
            userManager.RemoveUser(userSessionId);

            MainServer.MainLogger.Debug($"Client DisConnect / Now Session Count : {mainServer.SessionCount}");
        }
        // 로그인 요청

        // 로그인 응답


    }
}