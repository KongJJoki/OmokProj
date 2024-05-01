using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
using SqlKata.Compilers;
using SqlKata.Execution;
using SuperSocket.SocketBase.Logging;
using System.Data;

namespace OmokGameServer
{
    public class GameDB : IGameDB
    {
        ILog mainLogger;
        DBConfig dbConfig;
        IDbConnection dbConnection;
        QueryFactory queryFactory;

        public GameDB(ILog mainLogger, DBConfig dbConfig)
        {
            this.mainLogger = mainLogger;
            this.dbConfig = dbConfig;
            try
            {
                dbConnection = new MySqlConnection(this.dbConfig.GameDB);
                dbConnection.Open();
                //mainLogger.Debug("DB 연결");
            }
            catch (Exception ex)
            {
                mainLogger.Error(ex.ToString());
            }
            MySqlCompiler compiler = new MySqlCompiler();
            queryFactory = new QueryFactory(dbConnection, compiler);
        }

        public void Dispose()
        {
            dbConnection.Close();
        }

        public async Task<int> GetWinCount(string userId)
        {
            try
            {
                return await queryFactory.Query("usergamedata2")
                    .Select("winCount")
                    .Where("userName", userId)
                    .FirstOrDefaultAsync<int>();
            }
            catch (Exception ex)
            {
                mainLogger.Error(ex.ToString());
                return -1;
            }
        }
        public async Task<int> GetLoseCount(string userId)
        {
            try
            {
                return await queryFactory.Query("usergamedata2")
                    .Select("loseCount")
                    .Where("userName", userId)
                    .FirstOrDefaultAsync<int>();
            }
            catch (Exception ex)
            {
                mainLogger.Error(ex.ToString());
                return -1;
            }
        }

        public async Task UpdateWinCount(string userId, int originWinCount)
        {
            try
            {
                await queryFactory.Query("usergamedata2")
                        .Where("userName", userId)
                        .AsUpdate(new { winCount = originWinCount + 1 })
                        .FirstOrDefaultAsync<int>();
            }
            catch (Exception ex)
            {
                mainLogger.Error(ex.ToString());
            }

        }

        public async Task UpdateLoseCount(string userId, int originLoseCount)
        {
            try
            {
                await queryFactory.Query("usergamedata2")
                        .Where("userName", userId)
                        .AsUpdate(new { loseCount = originLoseCount + 1 })
                        .FirstOrDefaultAsync<int>();
            }
            catch (Exception ex)
            {
                mainLogger.Error(ex.ToString());
            }

        }
    }
}