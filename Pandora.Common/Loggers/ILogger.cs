using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Common.Loggers
{
    public interface ILogger
    {
        void LogMessage(string message);
        TextWriter GetOutputStream();
    }
}
