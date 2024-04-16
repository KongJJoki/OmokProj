using Hive_Server.Repository;
using Hive_Server.Model;
using Hive_Server.Services.Interface;
using MySqlConnector;
using Hive_Server.Model.DTO;
using Hive_Server;

namespace Hive_Server.Services
{
    public class AccountCreateService : IAccountCreateService
    {
        private IAccountDB accountDB;

        public AccountCreateService(IAccountDB accountDB)
        {
            this.accountDB = accountDB;
        }

        public async Task<EErrorCode> AccountCreate(string email, string password)
        {
            try
            {
                string hashedPassword = Hashing.HashingPassword(password);

                int insertCount = await accountDB.InsertAccount(email, hashedPassword);

                if(insertCount != 1)
                {
                    return EErrorCode.AccountCreateFail;
                }
                else
                {
                    return EErrorCode.None;
                }
            }
            catch(MySqlException dbEx)
            {
                return EErrorCode.DBError;
            }
            catch(Exception ex)
            {
                return EErrorCode.AccountCreateFail;
            }
        }
    }
}