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
            packetHandlerDictionary.Add((int)PACKET_ID.InNTFClientDisconnect, InterNTFClientDisconnect);
            packetHandlerDictionary.Add((int)PACKET_ID.LoginRequest, LoginRequest);
        }
        // 클라이언트 연결
        public void InternalNTFClientConnect(ServerPacketData packetData)
        {
            mainLogger.Debug($"New Client Connect");
        }
        // 클라이언트 연결 종료
        public void InterNTFClientDisconnect(ServerPacketData packetData)
        {
            // 패킷 데이터에서 유저 정보 가져오기
            string userSessionId = packetData.SessionId;

            ERROR_CODE userExistCheck = userManager.CheckUserExist(userSessionId);
            if (userExistCheck == ERROR_CODE.None)
            {
                userManager.RemoveUser(userSessionId);
            }

            var user = userManager.GetUser(userSessionId);
            if (user.isInRoom)
            {
                var room = roomList[user.roomNumber - serverOption.RoomStartNumber];
                room.RemoveUser(user);
            }

            mainLogger.Debug($"Client DisConnect");
        }
        // 로그인 요청
        public void LoginRequest(ServerPacketData packet)
        {
            string sessionId = packet.SessionId;
            mainLogger.Info($"Login Request : sessionId({sessionId})");

            try
            {
                if (userManager.CheckUserExist(sessionId) == ERROR_CODE.Login_Fail_Already_Exist_Session)
                {
                    LoginRespond(ERROR_CODE.Login_Fail_Already_Exist_Session, sessionId);
                    mainLogger.Debug($"sessionId({sessionId}) Login Fail : Error({ERROR_CODE.Login_Fail_Already_Exist_Session})");
                    return;
                }

                var requestData = MemoryPackSerializer.Deserialize<PKTReqLogin>(packet.BodyData);

                ERROR_CODE errorCode = userManager.AddUser(sessionId, requestData.UserId);

                if (errorCode == ERROR_CODE.Login_Fail_User_Count_Limit_Exceed)
                {
                    FullUserRespond(errorCode, sessionId);
                    mainLogger.Debug($"sessionId({sessionId}) Login Fail : Error({errorCode})");
                }

                LoginRespond(errorCode, sessionId);
                mainLogger.Debug($"sessionId({sessionId}) Login Success");
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
            loginRes.Result = (int)errorCode;

            var bodyData = MemoryPackSerializer.Serialize(loginRes);
            var sendData = PacketToBytes.MakeBytes(PACKET_ID.LoginRespond, bodyData);

            sendFunc(sessionId, sendData);
        }
        // 유저 다 찬 경우 보낼 응답
        public void FullUserRespond(ERROR_CODE errorCode, string sessionId)
        {
            PKTResFullUser userFullRes = new PKTResFullUser();
            userFullRes.Result = (int)errorCode;

            var bodyData = MemoryPackSerializer.Serialize(userFullRes);
            var sendData = PacketToBytes.MakeBytes(PACKET_ID.FullUser, bodyData);

            sendFunc(sessionId, sendData);
        }
    }
}