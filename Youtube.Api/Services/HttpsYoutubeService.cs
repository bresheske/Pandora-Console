using Pandora.Common.Loggers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Youtube.Api.Dtos;

namespace Youtube.Api.Services
{
    /// <summary>
    /// No intentions of using the offical API, just scrapeing the web like a boss.
    /// I'm even not using an XML parser; using full on regex.  I'm such a great programmer.
    /// </summary>
    public class HttpsYoutubeService : IYoutubeService
    {
        private const string BASE_URL = "https://www.youtube.com/results?search_query=";
        private const string BASE_VIDEO_URL = "https://www.youtube.com";

        private readonly ILogger _logger;

        public HttpsYoutubeService(ILogger logger)
        {
            _logger = logger;
        }

        public SearchResponse Search(SearchRequest request)
        {
            var output = new SearchResponse();
            try
            {
                using (var client = new WebClient())
                {
                    var url = $"{BASE_URL}{request.SearchText}";
                    var searchhtml = client.DownloadString(url);
                    var matches = Regex.Matches(searchhtml, @"/watch\?v=[\dA-Za-z\-_]*");
                    foreach (Match m in matches)
                    {
                        output.SearchResults.Add(BASE_VIDEO_URL + m.Value);
                    }
                    output.SearchResults = output.SearchResults.Distinct().ToList();

                    output.Successful = true;
                }
            }
            catch(Exception ex)
            {
                _logger.LogMessage($"ERROR (HttpsYoutubeSearchService,Search): {ex.Message}");
                output.Successful = false;
            }

            return output;
        }

        public void Download(string url)
        {
            try
            {
                var startinfo = new ProcessStartInfo()
                {
                    Arguments = "--extract-audio --audio-format mp3 " + url,
                    FileName = "youtube-dl",
                    UseShellExecute = false
                };
                var proc = Process.Start(startinfo);
                proc.WaitForExit();
            }
            catch (Exception ex)
            {
                _logger.LogMessage($"ERROR (HttpsYoutubeSearchService,Download): {ex.Message}");
            }
        }
    }
}
