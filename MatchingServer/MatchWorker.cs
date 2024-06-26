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

        public MatchConfig GetCompleteMatching(int uid);
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

        RedisList<MatchReqForm> matchReqList;
        RedisList<MatchCompeleteForm> matchCompleteList;

        string matchReqListKey = "";
        string matchCompleteListKey = "";

        public MatchWoker(IOptions<RedisDBConfig> redisDBConfig)
        {
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

        public MatchConfig GetCompleteMatching(int uid)
        {
            _completeDic.TryGetValue(uid, out var matchingDataInfo);

            _completeDic.TryRemove(uid, out _);

            return matchingDataInfo;
        }

        public record MatchReqForm(int user1Uid, int user2Uid);
        public record MatchCompeleteForm(int user1Uid, int user2Uid, string IP, string port, int roomNum);

        async void RunMatching()
        {
            matchReqList = new RedisList<MatchReqForm>(redisConnMatchReq, matchReqListKey, defaultExpireTime);

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

                    await matchReqList.RightPushAsync(matchReqData);
                }
                catch (Exception ex)
                {

                }
            }
        }

        void RunMatchingComplete()
        {
            matchCompleteList = new RedisList<MatchCompeleteForm>(redisConnMatchComplete, matchCompleteListKey, defaultExpireTime);

            while (true)
            {
                try
                {
                    var result = matchCompleteList.LeftPopAsync().Result;

                    if (result.HasValue == false)
                    {
                        Thread.Sleep(250);
                        continue;
                    }

                    var matchCompeleteRes = result.Value;

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