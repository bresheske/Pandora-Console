using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Api.Dtos
{
    public class StationDetailRequest : BaseRequestDto
    {
        public string StationToken { get; set; }
    }
}
