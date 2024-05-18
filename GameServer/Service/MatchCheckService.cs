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
    public class MatchCheckService : IMatchCheckService
    {
        string matchCheckUrl;
        private readonly HttpClient httpClient;

        public MatchCheckService(IConfiguration serverconfig)
        {
            matchCheckUrl = serverconfig.GetSection("MatchingServer").Value + serverconfig.GetSection("MatchCheckUrl").Value;
            this.httpClient = new HttpClient();
        }

        public async Task<MatchCheckRes> MatchCheck(int uid)
        {
            MatchCheckRes res = new MatchCheckRes();

            try
            {
                MatchCheckReq requestData = new MatchCheckReq
                {
                    Uid = uid
                };

                res = await HttpRequest(matchCheckUrl, requestData);

                return res;
            }
            catch (Exception ex)
            {
                res.Result = EErrorCode.MatchReqFail;
                res.SockIP = "";
                res.SockPort = "";
                res.RoomNum = 0;

                return res;
            }
        }

        public async Task<MatchCheckRes> HttpRequest(string requestUrl, MatchCheckReq requestData)
        {
            string requestBody = JsonSerializer.Serialize(requestData);

            HttpResponseMessage response = await httpClient.PostAsync(requestUrl,
                new StringContent(requestBody, Encoding.UTF8, "application/json"));

            MatchCheckRes res = new MatchCheckRes();

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();

                JsonDocument jsonDocument = JsonDocument.Parse(responseBody);
                JsonElement jsonResult = jsonDocument.RootElement;

                int resultValue = jsonResult.GetProperty("result").GetInt32();

                if (resultValue != 0)
                {
                    res.Result = (EErrorCode)resultValue;
                    res.SockIP = "";
                    res.SockPort = "";
                    res.RoomNum = 0;

                    return res;
                }
                else
                {
                    res.Result = (EErrorCode)resultValue;
                    res.SockIP = jsonResult.GetProperty("sockIP").GetString();
                    res.SockPort = jsonResult.GetProperty("sockPort").GetString();
                    res.RoomNum = jsonResult.GetProperty("roomNumber").GetInt32();

                    return res;
                }
            }
            else
            {
                res.Result = EErrorCode.HttpReqFail;
                res.SockIP = "";
                res.SockPort = "";

                return res;
            }
        }
    }
}