using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace LinkyLink.Controllers
{
    /// <summary>
    /// Manages endpoints for openid configuration pass through
    /// </summary>
    [Route("api/[controller]")]
    public class OpenIdController : Controller
    {
        private IConfigurationSection azureAdConfig;

        public OpenIdController(IConfiguration configuration)
        {
            this.azureAdConfig = configuration.GetSection("AzureAdB2C");
        }

        /// <summary>
        /// Get the tenant OpenID configuration
        /// </summary>
        /// <returns></returns>
        [HttpGet(".well-known/openid-configuration")]
        public async Task<IActionResult> Configuration()
        {
            var openIdConfigUrl = $"https://{azureAdConfig["Name"]}.b2clogin.com/{azureAdConfig["Domain"]}/v2.0/.well-known/openid-configuration?p={azureAdConfig["SignUpSignInPolicyId"]}";

            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(openIdConfigUrl);
                var json = JsonConvert.DeserializeObject<JObject>(await response.Content.ReadAsStringAsync());

                // The jwks_uri endpoint does not support cors - need to route this back through our own app.
                json["jwks_uri"] = $"{Request.Scheme}://{Request.Host}/api/openid/keys";

                return Ok(json);
            }
        }

        /// <summary>
        /// Gets the signing keys from the Azure AD B2C instance
        /// </summary>
        /// <returns></returns>
        [HttpGet("keys")]
        public async Task<IActionResult> SigningKeys()
        {
            var openIdConfigUrl = $"https://{azureAdConfig["Name"]}.b2clogin.com/{azureAdConfig["Domain"]}/discovery/v2.0/keys?p={azureAdConfig["SignUpSignInPolicyId"]}";

            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(openIdConfigUrl);
                var json = JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync());

                return Ok(json);
            }
        }
    }
}
