using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.Extensions.Logging;

namespace Moneteer.Landing.Helpers
{
    public class ApiClientHelper : IApiClientHelper
    {
        private readonly IConfigurationHelper _configurationHelper;
        private readonly ILogger<ApiClientHelper> _logger;

        public ApiClientHelper(IConfigurationHelper configurationHelper, ILogger<ApiClientHelper> logger)
        {
            _configurationHelper = configurationHelper;
            _logger = logger;
        }

        public async Task<string> GetAccessToken()
        {
            try
            {
                var client = new HttpClient();
                var disco = await client.GetDiscoveryDocumentAsync(_configurationHelper.IdentityUri);

                if (disco.IsError)
                {
                    throw new Exception(disco.Error);
                }

                var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
                {
                    Address = disco.TokenEndpoint,
                    ClientId = "moneteer-mvc",
                    ClientSecret = _configurationHelper.ClientSecret,
                    Scope = "moneteer-api"
                });

                if (tokenResponse.IsError)
                {
                    throw new Exception(tokenResponse.Error);
                }

                return tokenResponse.AccessToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to get access token for api");
                throw;
            }
        }

        public HttpClient GetClient(string token)
        {
            var client = new HttpClient();
            //client.SetBearerToken(token);
            client.BaseAddress = new Uri(_configurationHelper.ApiUri);

            return client;
        }
    }

    public interface IApiClientHelper
    {
        Task<string> GetAccessToken();
        HttpClient GetClient(string token);
    }
}
