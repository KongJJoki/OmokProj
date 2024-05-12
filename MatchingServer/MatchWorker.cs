using CloudStructures;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace MatchingServer
{

    public interface IMatchWoker : IDisposable
    {
        public void AddUser(string uid);

        public (bool, CompleteMatchingData) GetCompleteMatching(string uid);
    }

    public class MatchWoker : IMatchWoker
    {
        List<string> _pvpServerAddressList = new();

        Thread _reqWorker = null;
        ConcurrentQueue<string> _reqQueue = new();

        Thread _completeWorker = null;

        // key는 유저ID
        ConcurrentDictionary<string, string> _completeDic = new();

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

            redisConn1 = new RedisConnection(redisConfig);
            redisConn2 = new RedisConnection(redisConfig);




            //TODO: Redis 연결 및 초기화 한다


            _reqWorker = new System.Threading.Thread(this.RunMatching);
            _reqWorker.Start();

            _completeWorker = new System.Threading.Thread(this.RunMatchingComplete);
            _completeWorker.Start();
        }

        public void AddUser(string userID)
        {
            _reqQueue.Enqueue(userID);
        }

        public (bool, CompleteMatchingData) GetCompleteMatching(string userID)
        {
            //TODO: _completeDic에서 검색해서 있으면 반환한다.

            return (false, null);
        }

        void RunMatching()
        {
            while (true)
            {
                try
                {

                    if (_reqQueue.Count < 2)
                    {
                        System.Threading.Thread.Sleep(1);
                        continue;
                    }

                    //TODO: 큐에서 2명을 가져온다. 두명을 매칭시킨다

                    //TODO: Redis의 Pub/Sub을 이용해서 매칭된 유저들을 게임서버에 전달한다.


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

                    //TODO: 매칭 결과를 _completeDic에 넣는다
                    // 2명이 하므로 각각 유저를 대상으로 총 2개를 _completeDic에 넣어야 한다
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


    public class CompleteMatchingData
    {
        public string ServerAddress { get; set; }
        public int RoomNumber { get; set; }
    }
}