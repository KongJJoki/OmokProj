using MemoryPack;
using GameServerClientShare;
using InPacketTypes;
using SockInternalPacket;
using PacketDefine;

namespace OmokGameServer
{
    public class ConnectionLoginPacketHandler : PacketHandler
    {
        // 핸들러 맵에 함수 추가하는 함수
        public void SetPacketHandler(Dictionary<int, Action<ServerPacketData>> packetHandlerDictionary)
        {
            packetHandlerDictionary.Add((int)InPACKET_ID.InNTFClientConnect, InternalNTFClientConnect);
            packetHandlerDictionary.Add((int)InPACKET_ID.InNTFClientDisconnect, InternalNTFClientDisconnect);
            packetHandlerDictionary.Add((int)InPACKET_ID.InVerifiedLoginRequest, LoginRequest);
        }
        // 클라이언트 연결
        void InternalNTFClientConnect(ServerPacketData packetData)
        {
            // MaxConnectionNumber을 넘은 세션 접속은 슈퍼소켓에서 차단
            userManager.AddUserConnection(packetData.sessionId);

            mainLogger.Debug($"New Client Connect");
        }

        void InternalNTFClientDisconnect(ServerPacketData packetData)
        {
            string userSessionId = packetData.sessionId;

            var user = userManager.GetUser(userSessionId);
            /*if (user.isInRoom)
            {
                var room = roomManager.GetRoom(user.roomNumber);
                room.GameFinish();
                roomManager.EnqueueEmptyRoom(room.RoomNumber);
            }*/

            var room = roomManager.GetRoom(user.roomNumber);
            user.SetRoomLeave();
            room.GameFinish();
            if (room.nowUserCount == 0)
            {
                roomManager.EnqueueEmptyRoom(room.RoomNumber);
            }

            SockErrorCode userConnectedCheck = userManager.CheckUserConnected(userSessionId);
            if (userConnectedCheck == SockErrorCode.None)
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
        void LoginRequest(ServerPacketData packet)
        {
            string sessionId = packet.sessionId;
            mainLogger.Debug($"Login Request : sessionId({sessionId})");

            try 
            {
                var requestData = MemoryPackSerializer.Deserialize<InPKTVerifiedLoginReq>(packet.bodyData);

                if(requestData.ErrorCode != SockErrorCode.None)
                {
                    LoginRespond(requestData.ErrorCode, sessionId);
                    return;
                }

                SockErrorCode errorCodeCheck = userManager.CheckUserLoginExist(sessionId);

                if (errorCodeCheck != SockErrorCode.None)
                {
                    LoginRespond(errorCodeCheck, sessionId);
                    mainLogger.Debug($"sessionId({sessionId}) Login Fail : Error({SockErrorCode.LoginFailAlreadyExistUser})");
                    return;
                }

                SockErrorCode errorCodeLogin = userManager.AddUserLogin(sessionId, requestData.Uid);

                if (errorCodeLogin == SockErrorCode.LoginFailUserCountLimitExceed)
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

        void LoginRespond(SockErrorCode errorCode, string sessionId)
        {
            PKTResLogin loginRes = new PKTResLogin();
            loginRes.Result = errorCode;

            var bodyData = MemoryPackSerializer.Serialize(loginRes);
            var sendData = PacketToBytes.MakeToPacket(PACKET_ID.LoginResponse, bodyData);

            sendFunc(sessionId, sendData);
        }

        void FullUserRespond(SockErrorCode errorCode, string sessionId)
        {
            PKTResFullUser userFullRes = new PKTResFullUser();
            userFullRes.Result = errorCode;

            var bodyData = MemoryPackSerializer.Serialize(userFullRes);
            var sendData = PacketToBytes.MakeToPacket(PACKET_ID.FullUser, bodyData);

            sendFunc(sessionId, sendData);
        }
    }
}