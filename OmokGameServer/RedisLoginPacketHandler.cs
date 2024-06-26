using CloudStructures.Structures;
using CloudStructures;
using MemoryPack;
using PacketDefine;
using InPacketTypes;
using GameServerClientShare;
using SockInternalPacket;
using SqlKata.Execution;
using SuperSocket.SocketBase.Logging;
using MySqlX.XDevAPI.Common;

namespace OmokGameServer
{
    public class RedisLoginPacketHandler
    {
        ILog mainLogger;
        Action<ServerPacketData> passPacketToPacketProcessor;

        public void Init(ILog mainLogger, Action<ServerPacketData> passPacketToPacketProcessor)
        {
            this.mainLogger = mainLogger;
            this.passPacketToPacketProcessor = passPacketToPacketProcessor;
        }

        public void SetPacketHandler(Dictionary<int, Func<ServerPacketData, RedisConnection, Task>> redisPacketHandlerDictionary)
        {
            redisPacketHandlerDictionary.Add((int)PacketID.LoginRequest, VerifyLoginReq);
        }

        async Task VerifyLoginReq(ServerPacketData packet, RedisConnection redisConnection)
        {
            var requestData = MemoryPackSerializer.Deserialize<PKTReqLogin>(packet.bodyData);

            string realAuthToken = await GetAuthToken(requestData.Uid.ToString(), redisConnection);

            InPKTVerifiedLoginReq verifiedLoginReq = new InPKTVerifiedLoginReq();

            if (requestData.AuthToken == realAuthToken)
            {
                verifiedLoginReq.ErrorCode = SockErrorCode.None;
                verifiedLoginReq.Uid = requestData.Uid;
                verifiedLoginReq.AuthToken = requestData.AuthToken;
            }
            else
            {
                verifiedLoginReq.ErrorCode = SockErrorCode.LoginFailInvalidUser;
                verifiedLoginReq.Uid = 0;
                verifiedLoginReq.AuthToken = "";
            }

            var bodyData = MemoryPackSerializer.Serialize(verifiedLoginReq);

            ServerPacketData newPacket = new ServerPacketData();
            newPacket.SetPacket(packet.sessionId, (Int16)InPacketID.InVerifiedLoginRequest, bodyData);

            passPacketToPacketProcessor(newPacket);
        }

        async Task<string> GetAuthToken(string userId, RedisConnection redisConnection)
        {
            try
            {
                RedisString<string> redisString = new RedisString<string>(redisConnection, userId, null);
                RedisResult<string> authToken = await redisString.GetAsync();
                return authToken.Value;
            }
            catch (InvalidOperationException)
            {
                return "";
            }
        }
    }
}