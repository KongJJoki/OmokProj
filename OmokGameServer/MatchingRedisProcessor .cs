using SuperSocket.SocketBase.Logging;
using CloudStructures;
using NLog;
using CloudStructures.Structures;
using System.Text.Json;

namespace OmokGameServer
{
    class MatchingRedisProcessor
    {
        ILog mainLogger;
        MatchingConfig matchingConfig;
        ServerOption serverOption;
        RedisConfig matchingRedisConfig;

        string matchReqListKey = "";
        string matchCompleteListKey = "";

        RedisConnection redisConnMatchReq;
        RedisConnection redisConnMatchComplete;

        RedisList<MatchReqForm> matchReqList;
        RedisList<MatchCompeleteForm> matchCompleteList;

        TimeSpan defaultExpireTime = TimeSpan.FromDays(1);

        RoomManager roomManager;

        bool isMatchingRedisProcessorRunning;
        Thread matchingProcessorTh;

        public void ProcessorStart(ILog mainLogger, MatchingConfig matchingConfig, ServerOption serverOption, RoomManager roomManager)
        {
            this.mainLogger = mainLogger;
            this.matchingConfig = matchingConfig;
            this.serverOption = serverOption;
            matchingRedisConfig = new RedisConfig("MatchingRedis", this.matchingConfig.MatchingRedis);

            matchReqListKey = this.matchingConfig.MatchReqListKey;
            matchCompleteListKey = this.matchingConfig.MatchCompleteListKey;

            redisConnMatchReq = new RedisConnection(matchingRedisConfig);
            redisConnMatchComplete = new RedisConnection(matchingRedisConfig);

            this.roomManager = roomManager;

            isMatchingRedisProcessorRunning = true;
            matchingProcessorTh = new Thread(Process);
            matchingProcessorTh.Start();
        }

        public void ProcessorStop()
        {
            isMatchingRedisProcessorRunning = false;
        }

        public record MatchReqForm(int user1Uid, int user2Uid);
        public record MatchCompeleteForm(int user1Uid, int user2Uid, string IP, string port, int roomNum);

        void Process()
        {
            matchReqList = new RedisList<MatchReqForm>(redisConnMatchReq, matchReqListKey, defaultExpireTime);
            matchCompleteList = new RedisList<MatchCompeleteForm>(redisConnMatchComplete, matchCompleteListKey, defaultExpireTime);

            while (isMatchingRedisProcessorRunning)
            {
                try
                {
                    // 빈 방 있는지 체크
                    if(!roomManager.IsEmptyRoomExist())
                    {
                        continue;
                    }

                    // 레디스 리스트에서 값 있으면 가져오기
                    var result = matchReqList.LeftPopAsync().Result;
                    if(!result.HasValue)
                    {
                        Thread.Sleep(250);
                        continue;
                    }

                    var matchReqData = result.Value;
                    mainLogger.Debug($"매칭 요청 접수(user1 : {matchReqData.user1Uid} / user2 : {matchReqData.user2Uid})");

                    int emptyRoomNum = roomManager.GetEmptyRoomNum();
                    var matchCompeleteData = new MatchCompeleteForm(matchReqData.user1Uid, matchReqData.user2Uid, serverOption.IP.ToString(), serverOption.Port.ToString(), emptyRoomNum);

                    matchCompleteList.RightPushAsync(matchCompeleteData);
                    mainLogger.Debug($"매칭 결과 리스트에 추가(방 번호 : {emptyRoomNum})");
                }
                catch (Exception ex)
                {
                    mainLogger.Error($"MatchingRedisPacketProcessor Error : {ex}");
                }
            }
        }
    }
}