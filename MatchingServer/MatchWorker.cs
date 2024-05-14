using CloudStructures;
using CloudStructures.Structures;
using MatchingServer.Model.DTO;
using Microsoft.Extensions.Options;
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

            //TODO: Redis 연결 및 초기화 한다
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
            //TODO: _completeDic에서 검색해서 있으면 반환한다.
            if (_completeDic.ContainsKey(uid))
            {
                CompleteMatchData matchingData = new CompleteMatchData();
                matchingData.RoomNumber = _completeDic[uid];
                return (true, matchingData);
            }

            return (false, null);
        }

        public record MatchReqForm(int user1Uid, int user2Uid);
        public record MatchCompeleteForm(int roomNum, int user1Uid, int user2Uid);

        async void RunMatching()
        {
            // 키 값에 해당하는 리스트가 없으면 만들고, 있으면 가져옴
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

                    //TODO: 큐에서 2명을 가져온다. 두명을 매칭시킨다
                    int userUid1;
                    int userUid2;

                    _reqQueue.TryDequeue(out userUid1);
                    _reqQueue.TryDequeue(out userUid2);

                    var matchData = new MatchReqForm(user1Uid: userUid1, user2Uid: userUid2);
                    var jsonData = JsonSerializer.Serialize(matchData);

                    //TODO: Redis의 List를 이용해서 매칭 요청 List에 추가 -> 대전 서버 쪽에서 스레드로 돌면서 List에서 요소 가져오기(빈 방 있으면) -> 대전 서버가 다른 List에 요소 추가
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
                    //TODO: Redis의 List를 이용해서 매칭이 완료된 내용을 가져온다
                    var result = matchCompleteList.LeftPopAsync().Result;

                    if(result.HasValue == false)
                    {
                        continue;
                    }

                    var matchCompeleteRes = JsonSerializer.Deserialize<MatchCompeleteForm>(result.Value);

                    //TODO: 매칭 결과를 _completeDic에 넣는다
                    // 2명이 하므로 각각 유저를 대상으로 총 2개를 _completeDic에 넣어야 한다
                    _completeDic[matchCompeleteRes.user1Uid] = matchCompeleteRes.roomNum;
                    _completeDic[matchCompeleteRes.user2Uid] = matchCompeleteRes.roomNum;

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