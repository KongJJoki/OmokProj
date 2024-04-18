﻿using Hive_Server.Repository;
using Hive_Server.Services.Interface;
using Hive_Server.Model.DTO;
using Hive_Server;

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
                if(originToken == null) // 토큰이 존재하지 않는 경우
                {
                    return EErrorCode.TokenNotExist;
                }
                if(authToken != originToken) // 토큰이 달라서 유효하지 않은 경우
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