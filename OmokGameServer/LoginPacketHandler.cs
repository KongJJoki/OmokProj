using MemoryPack;
using PacketDefine;
using PacketTypes;

namespace OmokGameServer
{
    public class LoginPacketHandler : PacketHandler
    {
        // 핸들러 맵에 함수 추가하는 함수
        public void SetPacketHandler(Dictionary<int, Action<ServerPacketData>> packetHandlerDictionary)
        {
            packetHandlerDictionary.Add((int)PACKET_ID.IN_NTF_CLIENT_CONNECT, InternalNTFClientConnect);
            packetHandlerDictionary.Add((int)PACKET_ID.IN_NTF_CLIENT_DISCONNECT, InterNTFClientDisconnect);
            packetHandlerDictionary.Add((int)PACKET_ID.LOGIN_REQUEST, LoginRequest);
        }
        // 클라이언트 연결
        public void InternalNTFClientConnect(ServerPacketData packetData)
        {
            MainServer.MainLogger.Debug($"New Client Connect / Now Session Count : {MainServer.sessionCount}");
        }
        // 클라이언트 연결 종료
        public void InterNTFClientDisconnect(ServerPacketData packetData)
        {
            // 패킷 데이터에서 유저 정보 가져오기
            string userSessionId = packetData.SessionId;

            ERROR_CODE userExistCheck = userManager.CheckUserExist(userSessionId);
            if(userExistCheck == ERROR_CODE.None)
            {
                userManager.RemoveUser(userSessionId);
            }

            MainServer.MainLogger.Debug($"Client DisConnect / Now Session Count : {MainServer.sessionCount}");
        }
        // 로그인 요청
        public void LoginRequest(ServerPacketData packet)
        {
            string sessionId = packet.SessionId;
            MainServer.MainLogger.Info($"Login Request : sessionId({sessionId})");

            try
            {
                if(userManager.CheckUserExist(sessionId) == ERROR_CODE.Login_Fail_Already_Exist_Session)
                {
                    LoginRespond(ERROR_CODE.Login_Fail_Already_Exist_Session, sessionId);
                    MainServer.MainLogger.Debug($"sessionId({sessionId}) Login Fail : Error({ERROR_CODE.Login_Fail_Already_Exist_Session})");
                    return;
                }

                var requestData = MemoryPackSerializer.Deserialize<PKTReqLogin>(packet.BodyData);

                ERROR_CODE errorCode = userManager.AddUser(sessionId, requestData.UserId);

                if(errorCode == ERROR_CODE.Login_Fail_User_Count_Limit_Exceed)
                {
                    FullUserRespond(errorCode, sessionId);
                    MainServer.MainLogger.Debug($"sessionId({sessionId}) Login Fail : Error({errorCode})");
                }

                LoginRespond(errorCode, sessionId);
                MainServer.MainLogger.Debug($"sessionId({sessionId}) Login Success");
            }
            catch(Exception ex)
            {
                MainServer.MainLogger.Error(ex.ToString());
            }
        }
        // 로그인 응답
        public void LoginRespond(ERROR_CODE errorCode, string sessionId)
        {
            PKTResLogin loginRes = new PKTResLogin();
            loginRes.Result = (int)errorCode;

            var bodyData = MemoryPackSerializer.Serialize(loginRes);
            var sendData = PacketToBytes.MakeBytes(PACKET_ID.LOGIN_RESPOND, bodyData);

            sendFunc(sessionId, sendData);
        }
        // 유저 다 찬 경우 보낼 응답
        public void FullUserRespond(ERROR_CODE errorCode, string sessionId)
        {
            PKTResFullUser userFullRes = new PKTResFullUser();
            userFullRes.Result = (int)errorCode;

            var bodyData = MemoryPackSerializer.Serialize(userFullRes);
            var sendData = PacketToBytes.MakeBytes(PACKET_ID.FULL_USER, bodyData);

            sendFunc(sessionId, sendData);
        }
    }
}