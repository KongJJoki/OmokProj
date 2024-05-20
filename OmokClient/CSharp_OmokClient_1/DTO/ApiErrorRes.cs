using ErrorCode;

namespace OmokClient.DTO
{
    public class ApiErrorRes
    {
        public ApiErrorCode Result { get; set; } = ApiErrorCode.None;
    }
}
