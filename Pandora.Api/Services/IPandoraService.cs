using Pandora.Api.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Api.Services
{
    public interface IPandoraService
    {
        ListeningResponse CheckListening();
        PartnerLoginResponse PartnerLogin(PartnerLoginRequest request);
        UserLoginResponse UserLogin(UserLoginRequest request);
        StationDetailResponse GetStation(StationDetailRequest request);
    }
}
