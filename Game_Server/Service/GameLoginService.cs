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
        private readonly string serverUrl;
        private readonly IGameDB gameDB;
        private readonly IRedisDB redisDB;
        private readonly HttpClient httpClient;

        public GameLoginService(IGameDB gameDB, IRedisDB redisDB, IConfiguration serverconfig)
        {
            this.serverUrl = serverconfig.GetSection("HiveServer").Value;
            this.gameDB = gameDB;
            this.redisDB = redisDB;
            this.httpClient = new HttpClient();
        }

        public async Task<EErrorCode> GameLogin(Int32 userId, string authToken)
        {
            try
            {
                // Hive ������ ������ū ��ȿ�� �˻� ��û
                string verifyUrl = serverUrl + "/tokenverify";
                VerifyData verifyData = new VerifyData
                {
                    UserId = userId,
                    AuthToken = authToken
                };
                // ��û�� ���� Body ����ȭ
                string requestBody = Newtonsoft.Json.JsonConvert.SerializeObject(verifyData);

                HttpResponseMessage response = await httpClient.PostAsync(verifyUrl, 
                    new StringContent(requestBody, Encoding.UTF8, "application/json")); // ��û ������ ���� ���ڵ� + �̵�� Ÿ�� ����

                if(response.IsSuccessStatusCode) // �����ؼ� ���� ���� ���
                {
                    string responseBody = await response.Content.ReadAsStringAsync();

                    // ������ȭ JObject�� ���̺귯������ �����ϴ� JSON ��ü
                    JObject jsonResponse = JObject.Parse(responseBody);
                    int resultValue = (int)jsonResponse["result"];

                    if(resultValue != 0) // ������ū ��ȿ�� �˻� ����� ������ �ƴ� ���
                    {
                        return EErrorCode.InvalidToken;
                    }
                }
                else // ��û�� ������ ���
                {
                    return EErrorCode.HttpReqFail;
                }

                // ������ū ��ȿ�� �˻� ������ ���
                // Game_Server�� Redis�� ����
                bool redisSaveSuccess = await redisDB.InsertAuthToken(userId.ToString(), authToken);
                if(!redisSaveSuccess)
                {
                    return EErrorCode.RedisError;
                }

                bool isUserDataExist = await gameDB.GetUserDataExist(userId);
                if(!isUserDataExist) // �⺻������ ���� ���
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