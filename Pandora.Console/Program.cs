using NDesk.Options;
using Pandora.Api.Dtos;
using Pandora.Api.Services;
using Pandora.Common.Loggers;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Console
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Service settings.
            Pandora.Common.IOC.Container.Instance = new Container(x =>
            {
                x.For<ILogger>().Use<ConsoleLogger>();
                x.For<IPandoraService>().Use<HttpsPandoraService>();
                x.For<IEncryptionService>().Use<BlowfishEncryptionService>();
            });
            var container = Pandora.Common.IOC.Container.Instance;
            var logger = container.GetInstance<ILogger>();

            // options parsing
            var help = false;
            var list = false;
            var stationname = string.Empty;
            var username = string.Empty;
            var password = string.Empty;
            var set = new OptionSet()
            {

                {"list", "List Pandora Stations", o => list = true},
                {"h", "Help", o => help = true},
                {"s=", "Station to Read", o => stationname = o },
                {"u=", "Pandora Username (Email)", o => username = o },
                {"p=", "Pandora Password", o => password = o },
            };
            set.Parse(args);

            if (help)
            {
                set.WriteOptionDescriptions(logger.GetOutputStream());
                return;
            }

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                logger.LogMessage("Pandora Username and Password are required. Use -h for help.");
                return;
            }
            
            var service = container.GetInstance<IPandoraService>();

            // Check to make sure we can connect first.
            var response = service.CheckListening();
            if (!response.Successful)
                return;

            // Partner Login to get our ApiKey.
            var partnerusername = "winmo";
            var partnerpassword = "ED227E10a628EB0E8Pm825Dw7114AC39";
            var partnerdevice = "VERIZON_MOTOQ9C";
            var encryptionkey = "v93C8C2s12E0EBD";
            var decryptionkey = "7D671jt0C5E5d251";

            var partnerreq = new PartnerLoginRequest()
            {
                DeviceModel = partnerdevice,
                Password = partnerpassword,
                UserName = partnerusername,
                DecryptionKey = decryptionkey,
                EncryptionKey = encryptionkey
            };

            var partnerloginresponse = service.PartnerLogin(partnerreq);
            if (!response.Successful)
                return;

            // User Login
            var userreq = new UserLoginRequest()
            {
                DecryptionKey = decryptionkey,
                EncryptionKey = encryptionkey,
                PartnerAuthToken = partnerloginresponse.PartnerAuthToken,
                PartnerId = partnerloginresponse.PartnerId,
                Username = username,
                Password = password,
            };

            var userloginresponse = service.UserLogin(userreq);

            // Finally, process requests.

            // If we have list, and we specified a station name.
            if (!string.IsNullOrWhiteSpace(stationname) && list)
            {
                var station = userloginresponse.Stations
                    .FirstOrDefault(x => x.Name.ToLower() == stationname.ToLower());
                if (station == null)
                {
                    logger.LogMessage($"Error: Cannot find station '{stationname}'.");
                    return;
                }

                // Get the details of the station.
                var stationrequest = new StationDetailRequest()
                {
                    DecryptionKey = decryptionkey,
                    EncryptionKey = encryptionkey,
                    PartnerAuthToken = partnerloginresponse.PartnerAuthToken,
                    PartnerId = partnerloginresponse.PartnerId,
                    PartnerRequestSyncTime = partnerloginresponse.RequestSyncTime,
                    PartnerResponseSyncTime = partnerloginresponse.ResponseSyncTime,
                    StationToken = station.Token,
                    UserId = userloginresponse.UserId,
                    UserAuthToken = userloginresponse.UserAuthToken
                };

                var stationresponse = service.GetStation(stationrequest);
                foreach (var s in stationresponse.Songs)
                {
                    logger.LogMessage($"{s.Artist}:{s.Name}");
                }
                return;
            }
            // If we want to list, but didn't specify a station.
            else if (list)
            {
                foreach (var s in userloginresponse.Stations)
                {
                    logger.LogMessage($"{s.Id}:{s.Name}");
                }
            }
        }
    }
}
