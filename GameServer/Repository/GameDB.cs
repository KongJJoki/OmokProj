using Microsoft.Extensions.Options;
using MySqlConnector;
using SqlKata.Execution;
using System.Data;
using SqlKata.Compilers;

namespace GameServer.Repository
{
    public class GameDB : IGameDB
    {
        private readonly DBConfig dbConfig;
        private readonly IDbConnection dbConnection;
        private readonly QueryFactory queryFactory;

        public GameDB(IOptions<DBConfig> dbConfig) // DBConfig가 아니라 IOptions<DBConfig>로 주입받으면 실행 중에도 설정 값 업데이트 가능
        {
            this.dbConfig = dbConfig.Value; // IOptions<DBConfig> 내부의 값을 할당하기 위해 .Value
            dbConnection = new MySqlConnection(this.dbConfig.GameDB);
            MySqlCompiler compiler = new MySqlCompiler(); // mysql용 컴파일러 선언
            queryFactory = new QueryFactory(dbConnection, compiler);
        }

        public void Dispose()
        {
            dbConnection.Close();
        }

        public async Task<bool> GetUserDataExist(Int32 userId)
        {
            return await queryFactory.Query("usergamedata").Where("userId", userId).ExistsAsync();
        }

        public async Task<int> InsertBasicData(Int32 userId)
        {
            return await queryFactory.Query("usergamedata").InsertAsync(new
            {
                userID = userId,
                level = 1,
                exp = 0,
                winCount = 0,
                loseCount = 0
            }); // 기본 데이터 insert
        }

        public async Task<int> GetWinCount(Int32 userId)
        {
            return await queryFactory.Query("usergamedata")
                .Select("winCount")
                .Where("userId", userId)
                .FirstOrDefaultAsync<int>();
        }

        public async Task<int> GetLoseCount(Int32 userId)
        {
            return await queryFactory.Query("usergamedata")
                .Select("loseCount")
                .Where("userId", userId)
                .FirstOrDefaultAsync<int>();
        }

        public async Task<int> UpdateWinCount(Int32 userId, int originWinCount)
        {
            return await queryFactory.Query("usergamedata")
                    .Where("userName", userId)
                    .AsUpdate(new { winCount = originWinCount + 1 })
                    .FirstOrDefaultAsync<int>();
        }

        public async Task<int> UpdateLoseCount(Int32 userId, int originLoseCount)
        {
            return await queryFactory.Query("usergamedata")
                    .Where("userName", userId)
                    .AsUpdate(new { loseCount = originLoseCount + 1 })
                    .FirstOrDefaultAsync<int>();
        }
    }
}