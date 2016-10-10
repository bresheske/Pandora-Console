using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Youtube.Api.Dtos;

namespace Youtube.Api.Services
{
    public interface IYoutubeService
    {
        SearchResponse Search(SearchRequest request);
        void Download(string url);
    }
}
