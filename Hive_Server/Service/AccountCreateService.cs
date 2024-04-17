using Hive_Server.Repository;
using Hive_Server.Services.Interface;
using Hive_Server.Model.DTO;
using Hive_Server;
using System.Text.RegularExpressions;
using MySql.Data.MySqlClient;

namespace Hive_Server.Services
{
    public class AccountCreateService : IAccountCreateService
    {
        private readonly IAccountDB accountDB;

        public AccountCreateService(IAccountDB accountDB)
        {
            this.accountDB = accountDB;
        }

        public async Task<EErrorCode> AccountCreate(string email, string password)
        {
            try
            {
                // 이메일 형식인지 확인
                string emailPattern = @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$";
                Regex regex = new Regex(emailPattern);
                if (!regex.IsMatch(email))
                {
                    return EErrorCode.NotEmailForm;
                }

                // 이미 있는 계정인지 확인
                if (await accountDB.AccountExistCheck(email))
                {
                    return EErrorCode.AlreadyExistAccount;
                }

                // 패스워드 암호화
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