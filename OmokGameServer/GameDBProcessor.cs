using SuperSocket.SocketBase.Logging;

namespace OmokGameServer
{
    public class GameDBProcessor
    {
        ILog mainLogger;
        IGameDB gameDB;

        public async void HandleGameResultUpdate(object sender, GameWinEventArgs e)
        {
            await UpdateWinCount(e.WinUserId);
            await UpdateLoseCount(e.LoseUserId);
        }

        async Task<ERROR_CODE> UpdateWinCount(string userId)
        {
            int originWinCount = await gameDB.GetWinCount(userId);
            if (originWinCount == -1)
            {
                return ERROR_CODE.GameResultSaveFailDBError;
            }

            await gameDB.UpdateWinCount(userId, originWinCount);
            mainLogger.Debug($"{userId} win Count Update");
            return ERROR_CODE.None;
        }

        async Task<ERROR_CODE> UpdateLoseCount(string userId)
        {
            int originLoseCount = await gameDB.GetLoseCount(userId);
            if (originLoseCount == -1)
            {
                return ERROR_CODE.GameResultSaveFailDBError;
            }

            await gameDB.UpdateLoseCount(userId, originLoseCount);
            mainLogger.Debug($"{userId} lose Count Update");
            return ERROR_CODE.None;
        }

        public void ProcessorStart(ILog mainLogger, IGameDB gameDB)
        {
            this.mainLogger = mainLogger;
            this.gameDB = gameDB;
        }
    }
}