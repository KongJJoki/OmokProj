using GameServer.Repository;
using GameServer.Services.Interface;
using GameServer.Model.DTO;
using GameServer;
using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Options;
using System.Text;
using MySql.Data.MySqlClient;

namespace GameServer.Services
{
    public class GameResultSaveService : IGameResultSaveService
    {
        private readonly IGameDB gameDB;

        public GameResultSaveService(IGameDB gameDB)
        {
            this.gameDB = gameDB;
        }

        public async Task<EErrorCode> GameResultSave(Int32 winUserId, Int32 loseUserId)
        {
            try
            {
                int originWinCount = await gameDB.GetWinCount(winUserId);

                int isWinSaveSuccess = await gameDB.UpdateWinCount(winUserId, originWinCount);

                if(isWinSaveSuccess != 1)
                {
                    return EErrorCode.GameWinResultSaveFail;
                }

                int originLoseCount = await gameDB.GetLoseCount(loseUserId);

                int isLoseSaveSuccess = await gameDB.UpdateLoseCount(loseUserId, originLoseCount);

                if(isLoseSaveSuccess != 1)
                {
                    return EErrorCode.GameLoseResultSaveFail;
                }

                return EErrorCode.None;
            }
            catch (MySqlException dbEx)
            {
                return EErrorCode.DBError;
            }
            catch (Exception ex)
            {
                return EErrorCode.GameResultSaveFail;
            }
        }
    }
}