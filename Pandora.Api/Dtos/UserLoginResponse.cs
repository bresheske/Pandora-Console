using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Api.Dtos
{
    public class UserLoginResponse : BaseResponseDto
    {
        public List<Station> Stations { get; set; }
        public string UserAuthToken { get; set; }
        public string UserId { get; set; }
    }
}
