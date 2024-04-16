using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Hive_Server.Repository
{
    public class RedisDB : IRedisDB
    {
        private readonly DBConfig dbConfig;
        private readonly ConnectionMultiplexer redis;
        private readonly IDatabase redisDB;

        public RedisDB(IOptions<DBConfig> dbConfig) // DBConfig�� �ƴ϶� IOptions<DBConfig>�� ���Թ����� ���� �߿��� ���� �� ������Ʈ ����
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