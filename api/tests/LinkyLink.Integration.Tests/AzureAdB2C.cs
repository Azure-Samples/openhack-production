using Newtonsoft.Json;

namespace LinkyLink.Integration.Tests
{
    [JsonObject]
    class AzureAdB2C
    {
        [JsonProperty]
        public string Authority { get; set; }
        [JsonProperty]
        public string BaseAddress { get; set; }
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
