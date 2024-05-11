using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmokClient.DTO
{
    public class ApiLoginReq
    {
        public Int32 Uid { get; set; }
        public string AuthToken { get; set; }
    }

    public class ApiLoginRes : ApiErrorRes
    {
        public string SockIP { get; set; }
        public string SockPort { get; set; }
    }
}
