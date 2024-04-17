using Hive_Server.Repository;
using Hive_Server.Services.Interface;
using Hive_Server.Model.DTO;
using Hive_Server.Model.DAO;
using Hive_Server;
using System.Text.RegularExpressions;
using MySql.Data.MySqlClient;

namespace Hive_Server.Services
{
    public class HiveLoginService : IHiveLoginService
    {
        private readonly IAccountDB accountDB;
        private readonly IRedisDB redisDB;

        public HiveLoginService(IAccountDB accountDB, IRedisDB redisDB)
        {
            this.accountDB = accountDB;
            this.redisDB = redisDB;
        }

        public async Task<(EErrorCode, Int32?, string?)> HiveLogin(string email, string password)
        {
            try
            {
                // �̸��� �������� Ȯ��
                string emailPattern = @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$";
                Regex regex = new Regex(emailPattern);
                if (!regex.IsMatch(email))
                {
                    return (EErrorCode.NotEmailForm, null, null);
                }

                // �����ϴ� �������� Ȯ��
                if (!await accountDB.AccountExistCheck(email))
                {
                    return (EErrorCode.NotExistAccount, null, null);
                }

                // ���� ���� ������
                Account accountInfo = await accountDB.GetAccountInfo(email);
                string comparePassword = Hashing.HashingPassword(password);
                if(comparePassword != accountInfo.Password) // �н����尡 ����ġ�ϴ� ���
                {
                    return (EErrorCode.WrongPassword, null, null);
                }

                // ������ū ����
                string authToken = Hashing.MakeAuthToken(accountInfo.UserId);
                bool isAutoTokenSave = await redisDB.InsertAuthToken(accountInfo.UserId.ToString(), authToken); ;
                if(!isAutoTokenSave) // ���𽺿� ���� ���� ���
                {
                    return (EErrorCode.RedisError, null, null);
                }

                return (EErrorCode.None, accountInfo.UserId, authToken);
            }
            catch (MySqlException dbEx)
            {
                return (EErrorCode.DBError, null, null);
            }
            catch (Exception ex)
            {
                return (EErrorCode.HiveLoginFail, null, null);
            }
        }
    }
}