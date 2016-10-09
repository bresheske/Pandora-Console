using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Common.Loggers
{
    public class ConsoleLogger : ILogger
    {
        public TextWriter GetOutputStream()
        {
            return Console.Out;
        }

        public void LogMessage(string message)
        {
            System.Console.WriteLine(message);
        }
    }
}
