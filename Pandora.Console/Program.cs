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
using Youtube.Api.Services;

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
                x.For<IYoutubeService>().Use<HttpsYoutubeService>();
            });
            var container = Pandora.Common.IOC.Container.Instance;
            var logger = container.GetInstance<ILogger>();

            // options parsing
            var help = false;
            var list = false;
            var stationname = string.Empty;
            var username = string.Empty;
            var password = string.Empty;
            var search = string.Empty;
            var download = false;
            var set = new OptionSet()
            {

                {"l", "List Pandora Stations", o => list = true},
                {"h", "Help", o => help = true},
                {"s=", "Station to Read", o => stationname = o },
                {"u=", "Pandora Username (Email)", o => username = o },
                {"p=", "Pandora Password", o => password = o },
                {"search=", "Search for a Song or Artist", o => search = o },
                {"dl", "Download Results from Youtube. Reqires youtube-dl and FFMpeg in the PATH variable.", o => download = true },
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
            
            var pandoraservice = container.GetInstance<IPandoraService>();
            var youtubeservice = container.GetInstance<IYoutubeService>();

            // Check to make sure we can connect first.
            var listeningresponse = pandoraservice.CheckListening();
            if (!listeningresponse.Successful)
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

            var partnerloginresponse = pandoraservice.PartnerLogin(partnerreq);
            if (!partnerloginresponse.Successful)
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

            var userloginresponse = pandoraservice.UserLogin(userreq);
            if (!userloginresponse.Successful)
                return;

            // Finally, process requests.

            // if we want to download results from youtube.
            if (download)
            {
                // validate our input first.
                if (string.IsNullOrWhiteSpace(stationname))
                {
                    logger.LogMessage("Download requires a station name.");
                    return;
                }

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

                var stationresult = pandoraservice.GetStation(stationrequest);

                foreach (var s in stationresult.Songs)
                {
                    var youtubereq = new Youtube.Api.Dtos.SearchRequest()
                    {
                        SearchText = $"{s.Artist} {s.Name}"
                    };
                    var youtuberes = youtubeservice.Search(youtubereq);
                    // Trusting the first result for now.
                    var vid = youtuberes.SearchResults.FirstOrDefault();
                    if (vid != null)
                    {
                        youtubeservice.Download(vid);
                    }
                }

            }
            // If we just want to look for music
            else if (!string.IsNullOrWhiteSpace(search))
            {
                var searchrequest = new SearchRequest()
                {
                    DecryptionKey = decryptionkey,
                    EncryptionKey = encryptionkey,
                    PartnerAuthToken = partnerloginresponse.PartnerAuthToken,
                    PartnerId = partnerloginresponse.PartnerId,
                    UserAuthToken = userloginresponse.UserAuthToken,
                    UserId = userloginresponse.UserId,
                    PartnerRequestSyncTime = partnerloginresponse.RequestSyncTime,
                    PartnerResponseSyncTime = partnerloginresponse.ResponseSyncTime,
                    SearchText = search
                };
                var searchresult = pandoraservice.Search(searchrequest);
                foreach (var s in searchresult.Songs)
                {
                    logger.LogMessage($"Song:{s.Artist}:{s.Name}");
                }
                foreach (var a in searchresult.Artists)
                {
                    logger.LogMessage($"Artist:{a.Name}");
                }
            }
            // If we have list, and we specified a station name.
            else if (!string.IsNullOrWhiteSpace(stationname) && list)
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

                var stationresponse = pandoraservice.GetStation(stationrequest);
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
