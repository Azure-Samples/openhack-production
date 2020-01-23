using LinkyLink.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace LinkyLink.Integration.Tests
{
    [TestClass]
    public class LinkApiTests : IntegrationTest, IDisposable
    {
        private readonly HttpClient client;
        private readonly AzureAdB2C b2cConfig;

        public LinkApiTests()
        {
            this.b2cConfig = this.Configuration.GetSection("AzureAdB2C").Get<AzureAdB2C>();

            this.ValidateConfigurationItem(() => this.b2cConfig.BaseAddress, nameof(this.b2cConfig.BaseAddress));
            this.ValidateConfigurationItem(() => this.b2cConfig.Authority, nameof(this.b2cConfig.Authority));
            this.ValidateConfigurationItem(() => this.b2cConfig.ClientId, nameof(this.b2cConfig.ClientId));
            this.ValidateConfigurationItem(() => this.b2cConfig.Scope, nameof(this.b2cConfig.Scope));
            this.ValidateConfigurationItem(() => this.b2cConfig.Username, nameof(this.b2cConfig.Username));
            this.ValidateConfigurationItem(() => this.b2cConfig.Password, nameof(this.b2cConfig.Password));

            this.client = new HttpClient
            {
                BaseAddress = new Uri(this.b2cConfig.BaseAddress)
            };
        }

        [TestMethod]
        public async Task Links_API_CRUD_Operations()
        {
            // Generate an access token for a test user account
            var accessToken = await this.CreateAccessTokenAsync();
            this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            // Get all link bundles
            var (allLinksResponse, _) = await this.GetAllLinkBundles();
            Assert.AreEqual(HttpStatusCode.OK, allLinksResponse.StatusCode);

            // Create a new authenticated link bundle
            var (userPostLinkBundleResponse, linkBundle) = await this.CreateLinkBundle();
            Assert.AreEqual(HttpStatusCode.Created, userPostLinkBundleResponse.StatusCode);

            // Get the user link bundles
            var (userLinksResponse, bundles) = await this.GetUserLinkBundles();
            Assert.AreEqual(HttpStatusCode.OK, userLinksResponse.StatusCode);
            Assert.IsTrue(bundles.Length > 0);

            // Get the newly created link bundle
            var (getLinkBundleResponse, userLinkBundle) = await this.GetLinkBundleByVanityUrl(linkBundle.VanityUrl);
            Assert.AreEqual(HttpStatusCode.OK, getLinkBundleResponse.StatusCode);
            Assert.AreEqual(linkBundle.VanityUrl, userLinkBundle.VanityUrl);
            Assert.AreEqual(linkBundle.UserId, userLinkBundle.UserId);
            Assert.AreEqual(linkBundle.Description, userLinkBundle.Description);
            Assert.AreEqual(linkBundle.Links.Count, userLinkBundle.Links.Count);

            // Delete the recently created link bundle
            var deleteLinkBundleResponse = await this.DeleteLinkBundle(linkBundle.VanityUrl);
            Assert.AreEqual(HttpStatusCode.NoContent, deleteLinkBundleResponse.StatusCode);

            // Ensure deleted bundle now returns 404
            var (getLinkAfterDeleteResponse, _) = await this.GetLinkBundleByVanityUrl(linkBundle.VanityUrl);
            Assert.AreEqual(HttpStatusCode.NotFound, getLinkAfterDeleteResponse.StatusCode);
        }

        /// <summary>
        /// Gets all link bundles from the Links API
        /// </summary>
        /// <returns></returns>
        private async Task<(HttpResponseMessage, LinkBundle[])> GetAllLinkBundles()
        {
            var response = await this.client.GetAsync("/api/links");
            var bundles = JsonConvert.DeserializeObject<LinkBundle[]>(await response.Content.ReadAsStringAsync());

            return (response, bundles);
        }

        /// <summary>
        /// Creates a new LinkBundle for the authenticated user
        /// </summary>
        /// <returns></returns>
        private async Task<(HttpResponseMessage, LinkBundle)> CreateLinkBundle()
        {
            var now = DateTime.UtcNow;
            var linkBundle = new LinkBundle
            {
                VanityUrl = $"int-test-{now.ToFileTimeUtc()}",
                UserId = this.b2cConfig.Username,
                Description = $"Link bundle created from unit test @ {now.ToLongDateString()}",
                Links = new List<Link>
                {
                    new Link {
                        Id = "https://www.microsoft.com",
                        Url = "https://www.microsoft.com",
                        Title = "Microsoft",
                        Description = "Description for Microsoft"
                    }
                }
            };

            var jsonContent = new StringContent(JsonConvert.SerializeObject(linkBundle), System.Text.Encoding.UTF8, "application/json");
            var response = await this.client.PostAsync($"/api/links", jsonContent);

            return (response, linkBundle);
        }

        /// <summary>
        /// Gets all LinkBundles for the authenticated user
        /// </summary>
        /// <returns></returns>
        private async Task<(HttpResponseMessage, LinkBundle[])> GetUserLinkBundles()
        {
            var userLinksResponse = await this.client.GetAsync($"/api/links/user/{this.b2cConfig.Username}");
            var bundles = JsonConvert.DeserializeObject<LinkBundle[]>(await userLinksResponse.Content.ReadAsStringAsync());

            return (userLinksResponse, bundles);
        }

        /// <summary>
        /// Gets a link bundle by the specified vanity url
        /// </summary>
        /// <param name="vanityUrl"></param>
        /// <returns></returns>
        private async Task<(HttpResponseMessage, LinkBundle)> GetLinkBundleByVanityUrl(string vanityUrl)
        {
            LinkBundle userLinkBundle = null;
            var response = await this.client.GetAsync($"/api/links/{vanityUrl}");

            if (response.StatusCode == HttpStatusCode.OK)
            {
                userLinkBundle = JsonConvert.DeserializeObject<LinkBundle>(await response.Content.ReadAsStringAsync());
            }

            return (response, userLinkBundle);
        }

        /// <summary>
        /// Deletes a link bundle by the specified vanity url
        /// </summary>
        /// <param name="vanityUrl"></param>
        /// <returns></returns>
        private async Task<HttpResponseMessage> DeleteLinkBundle(string vanityUrl)
        {
            return await this.client.DeleteAsync($"/api/links/{vanityUrl}");
        }

        private void ValidateConfigurationItem(Func<string> getConfigValue, string paramName)
        {
            if (string.IsNullOrWhiteSpace(getConfigValue()))
            {
                throw new ConfigurationErrorsException($"{paramName} is missing from configuration");
            }
        }

        /// <summary>
        /// Creates an Azure B2C access token for the configured user account
        /// </summary>
        /// <returns></returns>
        private async Task<string> CreateAccessTokenAsync()
        {
            var app = PublicClientApplicationBuilder
                .Create(this.b2cConfig.ClientId)
                .WithDefaultRedirectUri()
                .WithB2CAuthority(this.b2cConfig.Authority)
                .Build();

            var credentials = new NetworkCredential(this.b2cConfig.Username, this.b2cConfig.Password);

            var appScope = new List<string>(this.b2cConfig.Scope.Split(' '));

            var authResult = await app
                .AcquireTokenByUsernamePassword(appScope, credentials.UserName, credentials.SecurePassword)
                .ExecuteAsync();

            return authResult.AccessToken;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this.client.Dispose();
                }

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}
