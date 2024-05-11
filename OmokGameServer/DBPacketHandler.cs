using MemoryPack;
using PacketDefine;
using PacketTypes;
using SqlKata.Execution;
using SuperSocket.SocketBase.Logging;

namespace OmokGameServer
{
    public class DBPacketHandler
    {
        ILog mainLogger;

        public void Init(ILog mainLogger)
        {
            this.mainLogger = mainLogger;
        }

        public void SetPacketHandler(Dictionary<int, Action<ServerPacketData, QueryFactory>> dbPacketHandlerDictionary)
        {
            dbPacketHandlerDictionary.Add((int)PACKET_ID.InSaveGameResult, UpdateGameResult);
        }

        async void UpdateGameResult(ServerPacketData packet, QueryFactory queryFactory)
        {
            var requestData = MemoryPackSerializer.Deserialize<InPKTGameResult>(packet.bodyData);

            int originWinCount = GetWinCount(requestData.WinUserId, queryFactory);
            if(originWinCount < 0)
            {
                mainLogger.Debug($"userId({requestData.WinUserId})의 승리 횟수를 가져오는데 실패했습니다.");
                return;
            }

            int winSaveCheck = UpdateWinCount(requestData.WinUserId, originWinCount, queryFactory);
            if(winSaveCheck < 0)
            {
                mainLogger.Debug($"userId({requestData.WinUserId})의 승리 횟수를 저장하는데 실패했습니다.");
                return;
            }

            int originLoseCount = GetLoseCount(requestData.LoseUseId, queryFactory);
            if (originLoseCount < 0)
            {
                mainLogger.Debug($"userId({requestData.LoseUseId})의 패배 횟수를 가져오는데 실패했습니다.");
                return;
            }

            int loseSaveCheck = UpdateLoseCount(requestData.LoseUseId, originLoseCount, queryFactory);
            if (loseSaveCheck < 0)
            {
                mainLogger.Debug($"userId({requestData.LoseUseId})의 패배 횟수를 저장하는데 실패했습니다.");
                return;
            }
        }

        int GetWinCount(string userId, QueryFactory queryFactory)
        {
            try
            {
                return queryFactory.Query("usergamedata2")
                    .Select("winCount")
                    .Where("userName", userId)
                    .FirstOrDefault<int>();
            }
            catch (Exception ex)
            {
                mainLogger.Error(ex.ToString());
                return -1;
            }
        }

        int GetLoseCount(string userId, QueryFactory queryFactory)
        {
            try
            {
                return queryFactory.Query("usergamedata2")
                    .Select("loseCount")
                    .Where("userName", userId)
                    .FirstOrDefault<int>();
            }
            catch (Exception ex)
            {
                mainLogger.Error(ex.ToString());
                return -1;
            }
        }

        int UpdateWinCount(string userId, int originWinCount, QueryFactory queryFactory)
        {
            try
            {
                return queryFactory.Query("usergamedata2")
                        .Where("userName", userId)
                        .AsUpdate(new { winCount = originWinCount + 1 })
                        .FirstOrDefault<int>();
            }
            catch (Exception ex)
            {
                mainLogger.Error(ex.ToString());
                return -1;
            }

        }

        int UpdateLoseCount(string userId, int originLoseCount, QueryFactory queryFactory)
        {
            try
            {
                return queryFactory.Query("usergamedata2")
                        .Where("userName", userId)
                        .AsUpdate(new { loseCount = originLoseCount + 1 })
                        .FirstOrDefault<int>();
            }
            catch (Exception ex)
            {
                mainLogger.Error(ex.ToString());
                return -1;
            }

        }
    }
}