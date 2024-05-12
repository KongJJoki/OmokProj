using MemoryPack;
using PacketDefine;
using PacketTypes;
using SqlKata.Execution;
using SuperSocket.SocketBase.Logging;

namespace OmokGameServer
{
    public class DBGameResultSavePacketHandler
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

        void UpdateGameResult(ServerPacketData packet, QueryFactory queryFactory)
        {
            var requestData = MemoryPackSerializer.Deserialize<InPKTGameResult>(packet.bodyData);

            int originWinCount = GetWinCount(requestData.WinUserUid, queryFactory);
            if(originWinCount < 0)
            {
                mainLogger.Debug($"userId({requestData.WinUserUid})의 승리 횟수를 가져오는데 실패했습니다.");
                return;
            }

            int winSaveCheck = UpdateWinCount(requestData.WinUserUid, originWinCount, queryFactory);
            if(winSaveCheck < 0)
            {
                mainLogger.Debug($"userId({requestData.WinUserUid})의 승리 횟수를 저장하는데 실패했습니다.");
                return;
            }

            int originLoseCount = GetLoseCount(requestData.LoseUseUid, queryFactory);
            if (originLoseCount < 0)
            {
                mainLogger.Debug($"userId({requestData.LoseUseUid})의 패배 횟수를 가져오는데 실패했습니다.");
                return;
            }

            int loseSaveCheck = UpdateLoseCount(requestData.LoseUseUid, originLoseCount, queryFactory);
            if (loseSaveCheck < 0)
            {
                mainLogger.Debug($"userId({requestData.LoseUseUid})의 패배 횟수를 저장하는데 실패했습니다.");
                return;
            }
        }

        int GetWinCount(Int32 uid, QueryFactory queryFactory)
        {
            try
            {
                return queryFactory.Query("usergamedata")
                    .Select("winCount")
                    .Where("userName", uid)
                    .FirstOrDefault<int>();
            }
            catch (Exception ex)
            {
                mainLogger.Error(ex.ToString());
                return -1;
            }
        }

        int GetLoseCount(Int32 uid, QueryFactory queryFactory)
        {
            try
            {
                return queryFactory.Query("usergamedata")
                    .Select("loseCount")
                    .Where("userName", uid)
                    .FirstOrDefault<int>();
            }
            catch (Exception ex)
            {
                mainLogger.Error(ex.ToString());
                return -1;
            }
        }

        int UpdateWinCount(Int32 uid, int originWinCount, QueryFactory queryFactory)
        {
            try
            {
                return queryFactory.Query("usergamedata")
                        .Where("userName", uid)
                        .AsUpdate(new { winCount = originWinCount + 1 })
                        .FirstOrDefault<int>();
            }
            catch (Exception ex)
            {
                mainLogger.Error(ex.ToString());
                return -1;
            }

        }

        int UpdateLoseCount(Int32 uid, int originLoseCount, QueryFactory queryFactory)
        {
            try
            {
                return queryFactory.Query("usergamedata")
                        .Where("userName", uid)
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