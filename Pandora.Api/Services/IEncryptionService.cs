using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Api.Services
{
    public interface IEncryptionService
    {
        string Encrypt(string data, string password);
        string Decrypt(string data, string password);
    }
}
