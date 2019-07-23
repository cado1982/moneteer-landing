using Microsoft.Extensions.Configuration;

namespace Moneteer.Landing.Helpers
{
    public class ConfigurationHelper : IConfigurationHelper
    {
        private readonly IConfiguration _configuration;

        public ConfigurationHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string IdentityUri
        {
            get { return _configuration["IdentityUri"]; }
        }

        public string AppUri
        {
            get { return _configuration["AppUri"]; }
        }
    }
}
