using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Api.Dtos
{
    public class PartnerLoginRequest : BaseRequestDto
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string DeviceModel { get; set; }
    }
}
