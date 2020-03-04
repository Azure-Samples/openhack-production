using Newtonsoft.Json;

namespace LinkyLink.Integration.Tests
{
    /// <summary>
    /// Azure AD B2C Configuration for Integration tests
    /// </summary>
    [JsonObject]
    public class AzureAdB2C
    {
        [JsonProperty]
        public string Authority { get; set; }
        [JsonProperty]
        public string Username { get; set; }
        [JsonProperty]
        public string Password { get; set; }
        [JsonProperty]
        public string Scope { get; set; }
        [JsonProperty]
        public string ClientId { get; set; }
    }
}
