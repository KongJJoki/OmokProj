using Hive_Server.Repository;
using Hive_Server.Model.DTO;

namespace Hive_Server.Services.Interface
{
	public interface IAccountCreateService
    {
		public Task<EErrorCode> AccountCreate(string email, string password);
	}
}