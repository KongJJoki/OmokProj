using Microsoft.Extensions.Options;
using StackExchange.Redis;
using CloudStructures;
using CloudStructures.Structures;

namespace Hive_Server.Repository
{
    public class RedisDB : IRedisDB
    {
        private readonly DBConfig dbConfig;
        private readonly RedisConfig redisConfig;
        private readonly RedisConnection redisDB;

        public RedisDB(IOptions<DBConfig> dbConfig) // DBConfig가 아니라 IOptions<DBConfig>로 주입받으면 실행 중에도 설정 값 업데이트 가능
        {
            this.dbConfig = dbConfig.Value;
            redisConfig = new RedisConfig("redisDB", this.dbConfig.RedisDB);
            redisDB = new RedisConnection(redisConfig);
        }

        // Redis연결은 애플리케이션 종료 시 자동으로 닫힘 ConnectionMultiplexer가 관리

        public async Task<bool> InsertAuthToken(string userId, string authToken)
        {
            TimeSpan expireTime = TimeSpan.FromHours(24);
            string key = userId;
            string value = authToken;
            RedisString<string> redisString = new RedisString<string>(redisDB, key, expireTime);
            // SetAsync가 비동기 메서드라서 await 필요
            return await redisString.SetAsync(authToken); // SetAsync 성공 시 true 실패 시 false 반환
        }

        public async Task<string> GetAuthToken(string userId)
        {
            try
            {
                RedisString<string> redisString = new RedisString<string>(redisDB, userId, null); // 값을 가져오기만 하므로 만료시간 null
                RedisResult<string> result = await redisString.GetAsync();
                return result.Value;
            }
            catch (System.InvalidOperationException) // 존재하지 않는 키 조회 시 발생하는 예외
            {
                return null; // 예외 발생 시 null 반환으로 Service에서 에러코드 처리
            }
        }
    }
}