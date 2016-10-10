using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Youtube.Api.Dtos
{
    public class SearchResponse : BaseResponseDto
    {
        public SearchResponse()
        {
            SearchResults = new List<string>();
        }
        public List<string> SearchResults { get; set; }
    }
}
