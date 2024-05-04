using MemoryPack;
using PacketDefine;
using PacketTypes;

namespace OmokGameServer
{
    public class ConnectionLoginPacketHandler : PacketHandler
    {
        // 핸들러 맵에 함수 추가하는 함수
        public void SetPacketHandler(Dictionary<int, Action<ServerPacketData>> packetHandlerDictionary)
        {
            packetHandlerDictionary.Add((int)PACKET_ID.InNTFClientConnect, InternalNTFClientConnect);
            packetHandlerDictionary.Add((int)PACKET_ID.InNTFClientDisconnect, InternalNTFClientDisconnect);
            packetHandlerDictionary.Add((int)PACKET_ID.LoginRequest, LoginRequest);
        }
        // 클라이언트 연결
        public void InternalNTFClientConnect(ServerPacketData packetData)
        {
            // MaxConnectionNumber을 넘은 세션 접속은 슈퍼소켓에서 차단
            userManager.AddUserConnection(packetData.sessionId);

            mainLogger.Debug($"New Client Connect");
        }

        public void InternalNTFClientDisconnect(ServerPacketData packetData)
        {
            string userSessionId = packetData.sessionId;

            var user = userManager.GetUser(userSessionId);
            if (user.isInRoom)
            {
                var room = roomManager.GetRoom(user.roomNumber);
                room.GameFinish();
                room.RemoveUser(user);
            }

            ERROR_CODE userConnectedCheck = userManager.CheckUserConnected(userSessionId);
            if (userConnectedCheck == ERROR_CODE.None)
            {
                int userHeartBeatArrayIndex = userManager.GetUser(userSessionId).myUserArrayIndex;

                if(userManager.CheckUserExistInHeartBeatArray(userHeartBeatArrayIndex))
                {
                    userManager.RemoveUserFromArray(userHeartBeatArrayIndex);
                }

                userManager.RemoveUser(userSessionId);
            }

            mainLogger.Debug($"Client DisConnect");
        }
        // 로그인 요청
        public void LoginRequest(ServerPacketData packet)
        {
            string sessionId = packet.sessionId;
            mainLogger.Debug($"Login Request : sessionId({sessionId})");

            try 
            {
                ERROR_CODE errorCodeCheck = userManager.CheckUserLoginExist(sessionId);

                if (errorCodeCheck != ERROR_CODE.None)
                {
                    LoginRespond(errorCodeCheck, sessionId);
                    mainLogger.Debug($"sessionId({sessionId}) Login Fail : Error({ERROR_CODE.LoginFailAlreadyExistUser})");
                    return;
                }

                var requestData = MemoryPackSerializer.Deserialize<PKTReqLogin>(packet.bodyData);

                ERROR_CODE errorCodeLogin = userManager.AddUserLogin(sessionId, requestData.UserId);

                if (errorCodeLogin == ERROR_CODE.LoginFailUserCountLimitExceed)
                {
                    FullUserRespond(errorCodeLogin, sessionId);
                    mainLogger.Debug($"sessionId({sessionId}) Login Fail : Error({errorCodeLogin})");
                }

                LoginRespond(errorCodeLogin, sessionId);
                mainLogger.Info($"sessionId({sessionId}) Login Success");
            }
            catch (Exception ex)
            {
                mainLogger.Error(ex.ToString());
            }
        }
        // 로그인 응답
        void LoginRespond(ERROR_CODE errorCode, string sessionId)
        {
            PKTResLogin loginRes = new PKTResLogin();
            loginRes.Result = errorCode;

            var bodyData = MemoryPackSerializer.Serialize(loginRes);
            var sendData = PacketToBytes.MakeBytes(PACKET_ID.LoginResponse, bodyData);

            sendFunc(sessionId, sendData);
        }
        // 유저 다 찬 경우 보낼 응답
        public void FullUserRespond(ERROR_CODE errorCode, string sessionId)
        {
            PKTResFullUser userFullRes = new PKTResFullUser();
            userFullRes.Result = errorCode;

            var bodyData = MemoryPackSerializer.Serialize(userFullRes);
            var sendData = PacketToBytes.MakeBytes(PACKET_ID.FullUser, bodyData);

            sendFunc(sessionId, sendData);
        }
    }
}