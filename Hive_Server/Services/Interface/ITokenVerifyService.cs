using Hive_Server.Repository;
using Hive_Server.Model.DTO;

namespace Hive_Server.Services.Interface
{
    public interface ITokenVerifyService
    {
        public Task<EErrorCode> TokenVerify(string userId, string authToken);
    }
}