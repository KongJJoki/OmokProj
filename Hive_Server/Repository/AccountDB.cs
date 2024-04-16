using Microsoft.Extensions.Options;
using MySqlConnector;
using SqlKata.Execution;
using System.Data;
using SqlKata.Compilers;

namespace Hive_Server.Repository
{
    public class AccountDB : IAccountDB
    {
        private DBConfig dbConfig;
        private IDbConnection dbConnection;
        private QueryFactory queryFactory;

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
                    email = email, password = securePassword
                });
        }
    }
}