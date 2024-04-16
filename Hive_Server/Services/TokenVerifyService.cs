using Hive_Server.Repository;
using Hive_Server.Model;
using Hive_Server.Services.Interface;
using Hive_Server.Model.DTO;
using Hive_Server.Model.DAO;
using Hive_Server;
using System.Text.RegularExpressions;

namespace Hive_Server.Services
{
    public class TokenVerifyService : ITokenVerifyService
    {
        private readonly IRedisDB redisDB;

        public TokenVerifyService(IRedisDB redisDB)
        {
            this.redisDB = redisDB;
        }

        public async Task<EErrorCode> TokenVerify(string userId, string authToken)
        {
            try
            {
                string originToken = await redisDB.GetAuthToken(userId);
                if(originToken == null) // ��ū�� �������� �ʴ� ���
                {
                    return EErrorCode.TokenNotExist;
                }
                if(authToken != originToken) // ��ū�� �޶� ��ȿ���� ���� ���
                {
                    return EErrorCode.InvalidToken;
                }

                return EErrorCode.None;
            }
            catch (Exception ex)
            {
                return EErrorCode.TokenVerifyFail;
            }
        }
    }
}