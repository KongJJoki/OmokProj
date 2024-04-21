using Game_Server.Repository;
using Game_Server.Services.Interface;
using Game_Server.Model.DTO;
using Game_Server;
using System.Net.Http;
using Newtonsoft.Json;
using Microsoft.Extensions.Options;
using System.Text;
using Newtonsoft.Json.Linq;
using MySql.Data.MySqlClient;

namespace Game_Server.Services
{
    public class GameLoginService : IGameLoginService
    {
        private readonly string tokenVerifyUrl;
        private readonly IGameDB gameDB;
        private readonly IRedisDB redisDB;
        private readonly HttpClient httpClient;

        public GameLoginService(IGameDB gameDB, IRedisDB redisDB, IConfiguration serverconfig)
        {
            this.tokenVerifyUrl = serverconfig.GetSection("HiveServer").Value + serverconfig.GetSection("TokenVerifyUrl").Value;
            this.gameDB = gameDB;
            this.redisDB = redisDB;
            this.httpClient = new HttpClient();
        }

        public async Task<EErrorCode> GameLogin(Int32 userId, string authToken)
        {
            try
            {
                // Hive 서버에 인증토큰 유효성 검사 요청
                string verifyUrl = tokenVerifyUrl;
                VerifyData verifyData = new VerifyData
                {
                    UserId = userId,
                    AuthToken = authToken
                };
                // 요청에 보낼 Body 직렬화
                string requestBody = Newtonsoft.Json.JsonConvert.SerializeObject(verifyData);

                HttpResponseMessage response = await httpClient.PostAsync(verifyUrl, 
                    new StringContent(requestBody, Encoding.UTF8, "application/json")); // 요청 본문의 문자 인코딩 + 미디어 타입 지정

                if(response.IsSuccessStatusCode) // 성공해서 응답 받은 경우
                {
                    string responseBody = await response.Content.ReadAsStringAsync();

                    // 역직렬화 JObject는 라이브러리에서 제공하는 JSON 객체
                    JObject jsonResponse = JObject.Parse(responseBody);
                    int resultValue = (int)jsonResponse["result"];

                    if(resultValue != 0) // 인증토큰 유효성 검사 결과가 성공이 아닌 경우
                    {
                        return EErrorCode.InvalidToken;
                    }
                }
                else // 요청에 실패한 경우
                {
                    return EErrorCode.HttpReqFail;
                }

                // 인증토큰 유효성 검사 성공한 경우
                // Game_Server의 Redis에 저장
                bool redisSaveSuccess = await redisDB.InsertAuthToken(userId.ToString(), authToken);
                if(!redisSaveSuccess)
                {
                    return EErrorCode.RedisError;
                }

                bool isUserDataExist = await gameDB.GetUserDataExist(userId);
                if(!isUserDataExist) // 기본데이터 없는 경우
                {
                    int insertCount = await gameDB.InsertBasicData(userId);
                    if(insertCount != 1)
                    {
                        return EErrorCode.DBError;
                    }
                }

                return EErrorCode.None;
            }
            catch (MySqlException dbEx)
            {
                return EErrorCode.DBError;
            }
            catch (Exception ex)
            {
                return EErrorCode.GameLoginFail;
            }
        }
    }
}