using NUnit.Framework;
using Pandora.Api.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandora.Test.Pandora.Api.Tests
{
    [TestFixture]
    public class BlowfishEncryptionServiceTests
    {
        private readonly BlowfishEncryptionService _service;

        public BlowfishEncryptionServiceTests()
        {
            _service = new BlowfishEncryptionService();
        }

    }
}
