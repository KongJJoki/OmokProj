using HiveServer.Repository;
using HiveServer.Services.Interface;
using HiveServer.Model.DTO;
using HiveServer.Model.DAO;
using HiveServer;
using System.Text.RegularExpressions;
using MySql.Data.MySqlClient;

namespace HiveServer.Services
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

        public record HiveLoginResult(EErrorCode ErrorCode, int UserId, string AuthToken);

        public async Task<HiveLoginResult> HiveLogin(string email, string password)
        {
            try
            {
                // 이메일 형식인지 확인
                string emailPattern = @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$";
                Regex regex = new Regex(emailPattern);
                if (!regex.IsMatch(email))
                {
                    return new HiveLoginResult(ErrorCode: EErrorCode.NotEmailForm, UserId: null, AuthToken: null);
                }

                // 존재하는 계정인지 확인
                if (!await accountDB.AccountExistCheck(email))
                {
                    return new HiveLoginResult(ErrorCode: EErrorCode.NotExistAccount, UserId: null, AuthToken: null);
                }

                // 계정 정보 얻어오기
                Account accountInfo = await accountDB.GetAccountInfo(email);
                string comparePassword = Hashing.HashingPassword(password);
                if(comparePassword != accountInfo.Password) // 패스워드가 불일치하는 경우
                {
                    return new HiveLoginResult(ErrorCode: EErrorCode.WrongPassword, UserId: null, AuthToken: null);
                }

                // 인증토큰 생성
                string authToken = Hashing.MakeAuthToken(accountInfo.UserId);
                bool isAutoTokenSave = await redisDB.InsertAuthToken(accountInfo.UserId.ToString(), authToken); ;
                if(!isAutoTokenSave) // 레디스에 저장 못한 경우
                {
                    return new HiveLoginResult(ErrorCode: EErrorCode.RedisError, UserId: null, AuthToken: null);
                }

                return new HiveLoginResult(ErrorCode: EErrorCode.None, UserId: accountInfo.UserId, AuthToken: authToken);
            }
            catch (MySqlException dbEx)
            {
                return new HiveLoginResult(ErrorCode: EErrorCode.DBError, UserId: null, AuthToken: null);
            }
            catch (Exception ex)
            {
                return new HiveLoginResult(ErrorCode: EErrorCode.HiveLoginFail, UserId: null, AuthToken: null);
            }
        }
    }
}