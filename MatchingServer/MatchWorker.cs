using CloudStructures;
using CloudStructures.Structures;
using MatchingServer.Model.DTO;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Text.Json;

namespace MatchingServer
{
    public class MatchConfig
    {
        public string IP { get; set; }
        public string Port { get; set; }
        public int RoomNumber { get; set; }
    }

    public interface IMatchWoker : IDisposable
    {
        public void AddUser(int uid);

        public (bool, CompleteMatchData) GetCompleteMatching(int uid);
    }

    public class MatchWoker : IMatchWoker
    {
        Thread _reqWorker = null;
        ConcurrentQueue<int> _reqQueue = new();

        Thread _completeWorker = null;

        ConcurrentDictionary<int, MatchConfig> _completeDic = new();

        RedisDBConfig redisDBConfig;
        RedisConfig redisConfig;
        RedisConnection redisConnMatchReq;
        RedisConnection redisConnMatchComplete;

        TimeSpan defaultExpireTime = TimeSpan.FromDays(1);

        RedisList<string> matchReqList;
        RedisList<string> matchCompleteList;

        string matchReqListKey = "";
        string matchCompleteListKey = "";

        public MatchWoker(IOptions<RedisDBConfig> redisDBConfig)
        {
            Console.WriteLine("MatchWoker 생성자 호출");

            this.redisDBConfig = redisDBConfig.Value;
            redisConfig = new RedisConfig("redisDB", this.redisDBConfig.RedisDB);
            matchReqListKey = this.redisDBConfig.MatchReqListKey;
            matchCompleteListKey = this.redisDBConfig.MatchCompleteListKey;

            redisConnMatchReq = new RedisConnection(redisConfig);
            redisConnMatchComplete = new RedisConnection(redisConfig);

            _reqWorker = new Thread(RunMatching);
            _reqWorker.Start();

            _completeWorker = new Thread(RunMatchingComplete);
            _completeWorker.Start();
        }

        public void AddUser(int uid)
        {
            _reqQueue.Enqueue(uid);
        }

        public (bool, CompleteMatchData) GetCompleteMatching(int uid)
        {
            if (_completeDic.ContainsKey(uid))
            {
                CompleteMatchData matchingData = new CompleteMatchData();
                matchingData.SockIP = _completeDic[uid].IP;
                matchingData.SockPort = _completeDic[uid].Port;
                matchingData.RoomNumber = _completeDic[uid].RoomNumber;

                _completeDic.TryRemove(uid, out _);

                return (true, matchingData);
            }

            return (false, null);
        }

        public record MatchReqForm(int user1Uid, int user2Uid);
        public record MatchCompeleteForm(int user1Uid, int user2Uid, string IP, string port, int roomNum);

        async void RunMatching()
        {
            matchReqList = new RedisList<string>(redisConnMatchReq, matchReqListKey, defaultExpireTime);

            while (true)
            {
                try
                {
                    if (_reqQueue.Count < 2)
                    {
                        Thread.Sleep(1);
                        continue;
                    }

                    int userUid1;
                    int userUid2;

                    _reqQueue.TryDequeue(out userUid1);
                    _reqQueue.TryDequeue(out userUid2);

                    var matchReqData = new MatchReqForm(user1Uid: userUid1, user2Uid: userUid2);
                    var jsonData = JsonSerializer.Serialize(matchReqData);

                    await matchReqList.RightPushAsync(jsonData);
                }
                catch (Exception ex)
                {
                    
                }
            }
        }

        void RunMatchingComplete()
        {
            matchCompleteList = new RedisList<string>(redisConnMatchComplete, matchCompleteListKey, defaultExpireTime);

            while (true)
            {
                try
                {
                    var result = matchCompleteList.LeftPopAsync().Result;

                    if(result.HasValue == false)
                    {
                        continue;
                    }

                    var matchCompeleteRes = JsonSerializer.Deserialize<MatchCompeleteForm>(result.Value);

                    MatchConfig matchConfig = new MatchConfig();

                    matchConfig.IP = matchCompeleteRes.IP;
                    matchConfig.Port = matchCompeleteRes.port;
                    matchConfig.RoomNumber = matchCompeleteRes.roomNum;

                    _completeDic[matchCompeleteRes.user1Uid] = matchConfig;
                    _completeDic[matchCompeleteRes.user2Uid] = matchConfig;

                }
                catch (Exception ex)
                {

                }
            }
        }


        public void Dispose()
        {
            Console.WriteLine("MatchWoker 소멸자 호출");
        }
    }
}