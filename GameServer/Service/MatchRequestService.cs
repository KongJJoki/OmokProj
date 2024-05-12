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
    public class MatchRequestService : IMatchRequestService
    {
        string matchRequestUrl;
        private readonly HttpClient httpClient;

        public MatchRequestService(IConfiguration serverconfig)
        {
            matchRequestUrl = serverconfig.GetSection("MatchingServer").Value + serverconfig.GetSection("MatchRequestUrl").Value;
            this.httpClient = new HttpClient();
        }

        public async Task<EErrorCode> MatchRequest(int uid)
        {
            try
            {
                MatchRequest requestData = new MatchRequest
                {
                    Uid = uid
                };

                EErrorCode httpRespond = await HttpRequest(matchRequestUrl, requestData);
                if(httpRespond!=EErrorCode.None)
                {
                    return EErrorCode.MatchReqFail;
                }
                else
                {
                    return EErrorCode.None;
                }
            }
            catch (Exception ex)
            {
                return EErrorCode.MatchReqFail;
            }
        }

        public async Task<EErrorCode> HttpRequest(string requestUrl, MatchRequest requestData)
        {
            string requestBody = JsonSerializer.Serialize(requestData);

            HttpResponseMessage response = await httpClient.PostAsync(requestUrl,
                new StringContent(requestBody, Encoding.UTF8, "application/json"));

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();

                JsonDocument jsonDocument = JsonDocument.Parse(responseBody);
                JsonElement jsonResult = jsonDocument.RootElement;

                int resultValue = jsonResult.GetProperty("result").GetInt32();

                if (resultValue != 0)
                {
                    return (EErrorCode)resultValue;
                }
                else
                {
                    return EErrorCode.None;
                }
            }
            else
            {
                return EErrorCode.HttpReqFail;
            }
        }
    }
}