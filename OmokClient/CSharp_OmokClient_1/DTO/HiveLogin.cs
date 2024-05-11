using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmokClient.DTO
{
    public class HiveLoginReq
    {
        public string Id { get; set; }
        public string password { get; set; }
    }

    public class HiveLoginRes : HiveErrorRes
    {
        public Int32 Uid { get; set; }
        public string AuthToken { get; set; }
    }
}
