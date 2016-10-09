using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pandora.Api.Dtos;
using System.Net;
using Pandora.Common.Loggers;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace Pandora.Api.Services
{
    public class HttpsPandoraService : IPandoraService
    {
        private const string ROOT_URL_HTTPS = "https://tuner.pandora.com/services/json/?method=";
        private const string ROOT_URL_HTTP = "http://tuner.pandora.com/services/json/?method=";
        private const string PARTNER_LOGIN_METHOD = "auth.partnerLogin";
        private const string USER_LOGIN_METHOD = "auth.userLogin";
        private const string CHECK_LISTENING_METHOD = "test.checkLicensing";
        private const string STATION_DETAIL_METHOD = "station.getStation";

        private readonly StructureMap.Container _container;

        public HttpsPandoraService()
        {
            _container = Common.IOC.Container.Instance;
        }

        public ListeningResponse CheckListening()
        {
            var logger = _container.GetInstance<ILogger>();
            var output = new ListeningResponse();
            try
            {
                using (var client = new WebClient())
                {
                    // Form together our request object.
                    var url = $"{ROOT_URL_HTTP}{CHECK_LISTENING_METHOD}";

                    // POST the value off to the server.
                    var json = client.UploadString(url, string.Empty);

                    // Parse the results.
                    var token = JObject.Parse(json);
                    output.Successful = (bool)token.SelectToken("result.isAllowed") == true;
                }
            }
            catch (Exception ex)
            {
                logger.LogMessage($"ERROR (HttpPandoraService,CheckListening): {ex.Message}");
                output.Successful = false;
            }

            return output;
        }

        public PartnerLoginResponse PartnerLogin(PartnerLoginRequest request)
        {
            var logger = _container.GetInstance<ILogger>();
            var enc = _container.GetInstance<IEncryptionService>();
            var output = new PartnerLoginResponse();
            try
            {
                using (var client = new WebClient())
                {
                    // Form together our request object.
                    var url = $"{ROOT_URL_HTTPS}{PARTNER_LOGIN_METHOD}";
                    var obj = new JObject();
                    obj.Add("username", new JValue(request.UserName));
                    obj.Add("password", new JValue(request.Password));
                    obj.Add("deviceModel", new JValue(request.DeviceModel));
                    obj.Add("version", new JValue("5"));
                    var objstring = obj.ToString(Newtonsoft.Json.Formatting.None);

                    // POST the value off to the server.
                    output.RequestSyncTime = DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
                    var json = client.UploadString(url, objstring);

                    // Parse the results.
                    var token = JObject.Parse(json);
                    output.Successful = (string)token.SelectToken("stat") == "ok";
                    output.PartnerAuthToken = (string)token.SelectToken("result.partnerAuthToken");
                    output.PartnerId = (string)token.SelectToken("result.partnerId");
                    var time = (string)token.SelectToken("result.syncTime");
                    var dectime = enc.Decrypt(time, request.DecryptionKey);
                    output.ResponseSyncTime = long.Parse(Regex.Match(dectime.Substring(4), "^[0-9]*").Groups[0].Value);
                }
            }
            catch(Exception ex)
            {
                logger.LogMessage($"ERROR (HttpPandoraService,PartnerLogin): {ex.Message}");
                output.Successful = false;
            }

            return output;
        }

        public UserLoginResponse UserLogin(UserLoginRequest request)
        {
            var logger = _container.GetInstance<ILogger>();
            var encservice = _container.GetInstance<IEncryptionService>();
            var output = new UserLoginResponse();
            try
            {
                using (var client = new WebClient())
                {
                    // Form together our request object.
                    var url = $"{ROOT_URL_HTTPS}{USER_LOGIN_METHOD}&auth_token={Uri.EscapeDataString(request.PartnerAuthToken)}&partner_id={request.PartnerId}";
                    var obj = new JObject();
                    obj.Add("loginType", new JValue("user"));
                    obj.Add("username", new JValue(request.Username));
                    obj.Add("password", new JValue(request.Password));
                    obj.Add("partnerAuthToken", new JValue(request.PartnerAuthToken));
                    obj.Add("returnStationList", new JValue(true));
                    var now = DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
                    var synctime = now + request.PartnerRequestSyncTime - request.PartnerResponseSyncTime;
                    obj.Add("syncTime", new JValue(synctime));
                    var objstring = obj.ToString(Newtonsoft.Json.Formatting.None);
                    var encstring = encservice.Encrypt(objstring, request.EncryptionKey);

                    // POST the value off to the server.
                    var json = client.UploadString(url, encstring);

                    // Parse the results.
                    var token = JObject.Parse(json);
                    output.Successful = (string)token.SelectToken("stat") == "ok";
                    output.UserId = (string)token.SelectToken("result.userId");
                    output.UserAuthToken = (string)token.SelectToken("result.userAuthToken");
                    var stations = (JArray)token.SelectToken("result.stationListResult.stations");
                    output.Stations = new List<Station>();
                    foreach(var s in stations)
                    {
                        output.Stations.Add(new Station()
                        {
                            Id = (string)s.SelectToken("stationId"),
                            Token = (string)s.SelectToken("stationToken"),
                            Name = (string)s.SelectToken("stationName"),
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogMessage($"ERROR (HttpPandoraService,UserLogin): {ex.Message}");
                output.Successful = false;
            }

            return output;
        }

        public StationDetailResponse GetStation(StationDetailRequest request)
        {
            var logger = _container.GetInstance<ILogger>();
            var encservice = _container.GetInstance<IEncryptionService>();
            var output = new StationDetailResponse();
            try
            {
                using (var client = new WebClient())
                {
                    // Form together our request object.
                    var url = $"{ROOT_URL_HTTP}{STATION_DETAIL_METHOD}&auth_token={Uri.EscapeDataString(request.PartnerAuthToken)}&partner_id={request.PartnerId}&user_id={request.UserId}";
                    var obj = new JObject();
                    obj.Add("userAuthToken", new JValue(request.UserAuthToken));
                    obj.Add("stationToken", new JValue(request.StationToken));
                    obj.Add("includeExtendedAttributes", new JValue(true));
                    var now = DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
                    var synctime = now + request.PartnerRequestSyncTime - request.PartnerResponseSyncTime;
                    obj.Add("syncTime", new JValue(synctime));
                    var objstring = obj.ToString(Newtonsoft.Json.Formatting.None);
                    var encstring = encservice.Encrypt(objstring, request.EncryptionKey);

                    // POST the value off to the server.
                    var json = client.UploadString(url, encstring);

                    // Parse the results.
                    var token = JObject.Parse(json);
                    output.Successful = (string)token.SelectToken("stat") == "ok";
                    var thumbs = (JArray)token.SelectToken("result.feedback.thumbsUp");
                    output.Songs = new List<Song>();
                    foreach (var s in thumbs)
                    {
                        output.Songs.Add(new Song()
                        {
                            Name = (string)s.SelectToken("songName"),
                            Artist = (string)s.SelectToken("artistName"),
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogMessage($"ERROR (HttpPandoraService,UserLogin): {ex.Message}");
                output.Successful = false;
            }

            return output;
        }
    }
}
