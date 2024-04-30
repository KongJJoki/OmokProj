namespace HiveServer.Services.Interface
{
    public interface ITokenVerifyService
    {
        public Task<EErrorCode> TokenVerify(string userId, string authToken);
    }
}