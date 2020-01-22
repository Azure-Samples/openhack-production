using Microsoft.Extensions.Configuration;
using System.IO;

namespace LinkyLink.Integration.Tests
{
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
