using Microsoft.Extensions.Options;
using MySqlConnector;
using SqlKata.Execution;
using System.Data;
using SqlKata.Compilers;

namespace Game_Server.Repository
{
    public class GameDB : IGameDB
    {
        private readonly DBConfig dbConfig;
        private readonly IDbConnection dbConnection;
        private readonly QueryFactory queryFactory;

        public GameDB(IOptions<DBConfig> dbConfig) // DBConfig�� �ƴ϶� IOptions<DBConfig>�� ���Թ����� ���� �߿��� ���� �� ������Ʈ ����
        {
            this.dbConfig = dbConfig.Value; // IOptions<DBConfig> ������ ���� �Ҵ��ϱ� ���� .Value
            dbConnection = new MySqlConnection(this.dbConfig.GameDB);
            MySqlCompiler compiler = new MySqlCompiler(); // mysql�� �����Ϸ� ����
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
            }); // �⺻ ������ insert
        }
    }
}