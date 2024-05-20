using ErrorCode;

namespace OmokClient.DTO
{
    public class HiveErrorRes
    {
        public HiveErrorCode Result { get; set; } = HiveErrorCode.None;
    }
}
