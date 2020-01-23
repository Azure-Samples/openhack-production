using Microsoft.Extensions.Configuration;
using System.IO;

namespace LinkyLink.Integration.Tests
{
    /// <summary>
    /// Base class for running integration tests that use configuration provider
    /// </summary>
    public class IntegrationTest
    {
        protected IConfiguration Configuration { get; private set; }

        public IntegrationTest()
        {
            this.Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true, true)
                .AddEnvironmentVariables("INTTEST_")
                .Build();
        }
    }
}
