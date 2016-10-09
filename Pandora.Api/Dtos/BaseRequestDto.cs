using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Api.Dtos
{
    public class BaseRequestDto
    {
        public string PartnerAuthToken { get; set; }
        public string PartnerId { get; set; }
        public double PartnerRequestSyncTime { get; set; }
        public double PartnerResponseSyncTime { get; set; }
        public string UserId { get; set; }
        public string UserAuthToken { get; set; }
        public string EncryptionKey { get; set; }
        public string DecryptionKey { get; set; }
    }
}
