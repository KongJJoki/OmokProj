using GameServer.Repository;
using GameServer.Services.Interface;
using GameServer.Model.DTO;
using GameServer;
using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Options;
using System.Text;
using MySql.Data.MySqlClient;

namespace GameServer.Services
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

                EErrorCode httpRespond = await HttpRequest(verifyUrl, verifyData);
                if(httpRespond!=EErrorCode.None)
                {
                    return httpRespond;
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

        public async Task<EErrorCode> HttpRequest(string verifyUrl, VerifyData verifyData)
        {
            // 요청에 보낼 Body 직렬화
            string requestBody = JsonSerializer.Serialize(verifyData);

            HttpResponseMessage response = await httpClient.PostAsync(verifyUrl,
                new StringContent(requestBody, Encoding.UTF8, "application/json")); // 요청 본문의 문자 인코딩 + 미디어 타입 지정

            if (response.IsSuccessStatusCode) // 성공해서 응답 받은 경우
            {
                string responseBody = await response.Content.ReadAsStringAsync();

                // JsonDoument : JSON 데이터 읽고 파싱 JsonElemnt : JSON 데이터 접근 및 값 가져오기
                JsonDocument jsonDocument = JsonDocument.Parse(responseBody);
                JsonElement jsonResult = jsonDocument.RootElement;

                int resultValue = jsonResult.GetProperty("result").GetInt32();

                if (resultValue != 0) // 인증토큰 유효성 검사 결과가 성공이 아닌 경우
                {
                    return EErrorCode.InvalidToken;
                }
            }
            else // 요청에 실패한 경우
            {
                return EErrorCode.HttpReqFail;
            }

            return EErrorCode.None;
        }
    }
}