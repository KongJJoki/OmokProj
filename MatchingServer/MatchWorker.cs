using CloudStructures;
using MatchingServer.Model.DTO;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Collections.Concurrent;
using System.Text.Json;

namespace MatchingServer
{

    public interface IMatchWoker : IDisposable
    {
        public void AddUser(int uid);

        public (bool, CompleteMatchData) GetCompleteMatching(int uid);
    }

    public class MatchWoker : IMatchWoker
    {
        List<string> _pvpServerAddressList = new();

        Thread _reqWorker = null;
        ConcurrentQueue<int> _reqQueue = new();

        Thread _completeWorker = null;

        // key는 유저ID Value는 방 번호?
        ConcurrentDictionary<int, int> _completeDic = new();

        //TODO: 2개의 Pub/Sub을 사용하므로 Redis 객체가 2개 있어야 한다.
        // 매칭서버에서 -> 게임서버, 게임서버 -> 매칭서버로

        RedisDBConfig redisDBConfig;
        RedisConfig redisConfig;
        RedisConnection redisConn1;
        RedisConnection redisConn2;

        string pubKey = "";
        string subKey = "";

        public MatchWoker(IOptions<RedisDBConfig> redisDBConfig)
        {
            Console.WriteLine("MatchWoker 생성자 호출");

            this.redisDBConfig = redisDBConfig.Value;
            redisConfig = new RedisConfig("redisDB", this.redisDBConfig.RedisDB);
            pubKey = this.redisDBConfig.PubKey;
            subKey = this.redisDBConfig.SubKey;

            //TODO: Redis 연결 및 초기화 한다
            redisConn1 = new RedisConnection(redisConfig);
            redisConn2 = new RedisConnection(redisConfig);

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
            //TODO: _completeDic에서 검색해서 있으면 반환한다.
            if (_completeDic.ContainsKey(uid))
            {
                CompleteMatchData matchingData = new CompleteMatchData();
                matchingData.RoomNumber = _completeDic[uid];
                return (true, matchingData);
            }

            return (false, null);
        }

        public record MatchSuccessRes(int user1Uid, int user2Uid);

        void RunMatching()
        {
            while (true)
            {
                try
                {

                    if (_reqQueue.Count < 2)
                    {
                        Thread.Sleep(1);
                        continue;
                    }

                    //TODO: 큐에서 2명을 가져온다. 두명을 매칭시킨다
                    int userUid1;
                    int userUid2;

                    _reqQueue.TryDequeue(out userUid1);
                    _reqQueue.TryDequeue(out userUid2);

                    var matchData = new MatchSuccessRes(user1Uid: userUid1, user2Uid: userUid2);
                    var jsonData = JsonSerializer.Serialize(matchData);

                    //TODO: Redis의 Pub/Sub을 이용해서 매칭된 유저들을 게임서버에 전달한다.
                    var pub = redisConn1.GetConnection().GetSubscriber();
                    pub.Publish(pubKey, jsonData);

                }
                catch (Exception ex)
                {

                }
            }
        }

        void RunMatchingComplete()
        {
            while (true)
            {
                try
                {
                    //TODO: Redis의 Pub/Sub을 이용해서 매칭된 결과를 게임서버로 받는다
                    var sub = redisConn2.GetConnection().GetSubscriber();
                    sub.Subscribe(subKey, MatchingCompeleteHandler);

                    //TODO: 매칭 결과를 _completeDic에 넣는다
                    // 2명이 하므로 각각 유저를 대상으로 총 2개를 _completeDic에 넣어야 한다
                }
                catch (Exception ex)
                {

                }
            }
        }

        void MatchingCompeleteHandler(RedisChannel channel, RedisValue message)
        {
            // 받은 메세지 해석해서 _completeDic에 uid,방번호 넣기?
        }


        public void Dispose()
        {
            Console.WriteLine("MatchWoker 소멸자 호출");
        }
    }
}