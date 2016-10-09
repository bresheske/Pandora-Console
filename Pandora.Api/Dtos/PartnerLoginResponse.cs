using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Api.Dtos
{
    public class PartnerLoginResponse : BaseResponseDto
    {
        public string PartnerAuthToken { get; set; }
        public string PartnerId { get; set; }
        public int RequestSyncTime { get; internal set; }
        public int ResponseSyncTime { get; set; }
    }
}
