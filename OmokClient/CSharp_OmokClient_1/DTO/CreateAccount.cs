using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmokClient.DTO
{
    public class CreateAccountReq
    {
        public string Id { get; set; }
        public string password { get; set; }
    }

    public class CreateAccountRes : HiveErrorRes
    {

    }
}
