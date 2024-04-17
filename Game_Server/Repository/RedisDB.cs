using Microsoft.Extensions.Options;
using StackExchange.Redis;
using CloudStructures;
using CloudStructures.Structures;

namespace Game_Server.Repository
{
    public class RedisDB : IRedisDB
    {
        private readonly DBConfig dbConfig;
        private readonly RedisConfig redisConfig;
        private readonly RedisConnection redisDB;

        public RedisDB(IOptions<DBConfig> dbConfig) // DBConfig�� �ƴ϶� IOptions<DBConfig>�� ���Թ����� ���� �߿��� ���� �� ������Ʈ ����
        {
            this.dbConfig = dbConfig.Value;
            redisConfig = new RedisConfig("redisDB", this.dbConfig.RedisDB);
            redisDB = new RedisConnection(redisConfig);
        }

        // Redis������ ���ø����̼� ���� �� �ڵ����� ���� ConnectionMultiplexer�� ����

        public async Task<bool> InsertAuthToken(string userId, string authToken)
        {
            TimeSpan expireTime = TimeSpan.FromHours(24);
            string key = userId;
            string value = authToken;
            RedisString<string> redisString = new RedisString<string>(redisDB, key, expireTime);
            // SetAsync�� �񵿱� �޼���� await �ʿ�
            return await redisString.SetAsync(authToken); // SetAsync ���� �� true ���� �� false ��ȯ
        }
    }
}