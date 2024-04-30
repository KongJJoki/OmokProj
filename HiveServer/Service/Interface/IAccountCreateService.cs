namespace HiveServer.Services.Interface
{
	public interface IAccountCreateService
    {
		public Task<EErrorCode> AccountCreate(string email, string password);
	}
}