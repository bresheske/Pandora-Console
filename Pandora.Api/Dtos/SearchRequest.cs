using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Api.Dtos
{
    public class SearchRequest : BaseRequestDto
    {
        public string SearchText { get; set; }
    }
}
