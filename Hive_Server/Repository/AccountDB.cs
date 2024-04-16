using Microsoft.Extensions.Options;
using MySqlConnector;
using SqlKata.Execution;
using System.Data;
using SqlKata;
using SqlKata.Compilers;
using Hive_Server.Model.DAO;

namespace Hive_Server.Repository
{
    public class AccountDB : IAccountDB
    {
        private readonly DBConfig dbConfig;
        private readonly IDbConnection dbConnection;
        private readonly QueryFactory queryFactory;

        public AccountDB(IOptions<DBConfig> dbConfig) // DBConfig�� �ƴ϶� IOptions<DBConfig>�� ���Թ����� ���� �߿��� ���� �� ������Ʈ ����
        {
            this.dbConfig = dbConfig.Value; // IOptions<DBConfig> ������ ���� �Ҵ��ϱ� ���� .Value
            dbConnection = new MySqlConnection(this.dbConfig.AccountDB);
            MySqlCompiler compiler = new MySqlCompiler(); // mysql�� �����Ϸ� ����
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
                email = email,
                password = securePassword
            });
        }

        public async Task<bool> AccountExistCheck(string email)
        {
            return await queryFactory.Query("account").Where("email", email).ExistsAsync();
        }

        public async Task<Account> GetAccountInfo(string email)
        {
            return await queryFactory.Query("account")
                .Select("userId as UserId", "email as Email", "password as Password")
                .Where("email", email)
                .FirstOrDefaultAsync<Account>();
        }
    }
}