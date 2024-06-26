using MemoryPack;
using GameServerClientShare;
using InPacketTypes;
using SqlKata.Execution;
using SuperSocket.SocketBase.Logging;
using SockInternalPacket;

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
            dbPacketHandlerDictionary.Add((int)InPacketID.InSaveGameResult, UpdateGameResult);
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

        int GetWinCount(int uid, QueryFactory queryFactory)
        {
            try
            {
                return queryFactory.Query("usergamedata")
                    .Select("winCount")
                    .Where("userId", uid)
                    .FirstOrDefault<int>();
            }
            catch (Exception ex)
            {
                mainLogger.Error(ex.ToString());
                return -1;
            }
        }

        int GetLoseCount(int uid, QueryFactory queryFactory)
        {
            try
            {
                return queryFactory.Query("usergamedata")
                    .Select("loseCount")
                    .Where("userId", uid)
                    .FirstOrDefault<int>();
            }
            catch (Exception ex)
            {
                mainLogger.Error(ex.ToString());
                return -1;
            }
        }

        int UpdateWinCount(int uid, int originWinCount, QueryFactory queryFactory)
        {
            try
            {
                return queryFactory.Query("usergamedata")
                        .Where("userId", uid)
                        .AsUpdate(new { winCount = originWinCount + 1 })
                        .FirstOrDefault<int>();
            }
            catch (Exception ex)
            {
                mainLogger.Error(ex.ToString());
                return -1;
            }

        }

        int UpdateLoseCount(int uid, int originLoseCount, QueryFactory queryFactory)
        {
            try
            {
                return queryFactory.Query("usergamedata")
                        .Where("userId", uid)
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