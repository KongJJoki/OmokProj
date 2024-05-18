using OmokClient.CSCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmokClient.DTO
{
    public class ApiMatchCheckReq
    {
        
    }

    public class ApiMatchCheckRes : ApiErrorRes
    {
        public string SockIP { get; set; }
        public string SockPort { get; set; }
        public int RoomNum { get; set; }
    }
}
