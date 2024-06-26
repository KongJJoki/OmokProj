using Microsoft.Extensions.Options;
using MySqlConnector;
using SqlKata.Execution;
using System.Data;
using SqlKata;
using SqlKata.Compilers;
using HiveServer.Model.DAO;

namespace HiveServer.Repository
{
    public class AccountDB : IAccountDB
    {
        private readonly DBConfig dbConfig;
        private readonly IDbConnection dbConnection;
        private readonly QueryFactory queryFactory;

        public AccountDB(IOptions<DBConfig> dbConfig) // DBConfig가 아니라 IOptions<DBConfig>로 주입받으면 실행 중에도 설정 값 업데이트 가능
        {
            this.dbConfig = dbConfig.Value; // IOptions<DBConfig> 내부의 값을 할당하기 위해 .Value
            dbConnection = new MySqlConnection(this.dbConfig.AccountDB);
            MySqlCompiler compiler = new MySqlCompiler(); // mysql용 컴파일러 선언
            queryFactory = new QueryFactory(dbConnection, compiler);
        }

        public void Dispose()
        {
            dbConnection.Close();
        }

        public async Task<int> InsertAccount(string email, string securePassword)
        {
            return await queryFactory.Query("account").InsertAsync(new
            {
                id = email,
                password = securePassword
            });
        }

        public async Task<bool> AccountExistCheck(string id)
        {
            return await queryFactory.Query("account").Where("id", id).ExistsAsync();
        }

        public async Task<Account> GetAccountInfo(string id)
        {
            return await queryFactory.Query("account")
                .Select("userId as UserId", "id as Id", "password as Password")
                .Where("id", id)
                .FirstOrDefaultAsync<Account>();
        }
    }
}