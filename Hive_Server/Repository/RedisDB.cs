using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Hive_Server.Repository
{
    public class RedisDB : IRedisDB
    {
        private readonly DBConfig dbConfig;
        private readonly ConnectionMultiplexer redis;
        private readonly IDatabase redisDB;

        public RedisDB(IOptions<DBConfig> dbConfig) // DBConfig가 아니라 IOptions<DBConfig>로 주입받으면 실행 중에도 설정 값 업데이트 가능
        {
            this.dbConfig = dbConfig.Value;
            redis = ConnectionMultiplexer.Connect(this.dbConfig.RedisDB);
            redisDB = redis.GetDatabase();
        }

        public void Dispose()
        {
            redis.Dispose();
        }

        public async Task<bool> InsertAuthToken(string userId, string authToken)
        {
            TimeSpan expireTime = TimeSpan.FromHours(24);
            return await redisDB.StringSetAsync(userId, authToken, expireTime);
        }

        public async Task<string> GetAuthToken(string userId)
        {
            return await redisDB.StringGetAsync(userId);
        }
    }
}