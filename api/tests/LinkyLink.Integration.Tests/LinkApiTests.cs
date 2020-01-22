using LinkyLink.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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

            this.client = new HttpClient
            {
                BaseAddress = new Uri(this.b2cConfig.BaseAddress)
            };
        }

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

        [TestMethod]
        public async Task Links_API_CRUD_Operations()
        {
            var accessToken = await this.CreateAccessTokenAsync();
            this.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var allLinksResponse = await this.client.GetAsync("/api/links");
            Assert.AreEqual(HttpStatusCode.OK, allLinksResponse.StatusCode);

            // Create a new authenticated link bundle
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
            var userPostLinkBundleResponse = await this.client.PostAsync($"/api/links", jsonContent);

            Assert.AreEqual(HttpStatusCode.Created, userPostLinkBundleResponse.StatusCode);

            // Get the user link bundles
            var userLinksResponse = await this.client.GetAsync($"/api/links/user/{this.b2cConfig.Username}");
            var bundles = JsonConvert.DeserializeObject<LinkBundle[]>(await userLinksResponse.Content.ReadAsStringAsync());

            Assert.AreEqual(HttpStatusCode.OK, userLinksResponse.StatusCode);
            Assert.IsTrue(bundles.Length > 0);

            // Get the newly created link bundle
            var getLinkBundleResponse = await this.client.GetAsync($"/api/links/{linkBundle.VanityUrl}");
            var userLinkBundle = JsonConvert.DeserializeObject<LinkBundle>(await getLinkBundleResponse.Content.ReadAsStringAsync());

            Assert.AreEqual(HttpStatusCode.OK, getLinkBundleResponse.StatusCode);
            Assert.AreEqual(linkBundle.VanityUrl, userLinkBundle.VanityUrl);
            Assert.AreEqual(linkBundle.UserId, userLinkBundle.UserId);
            Assert.AreEqual(linkBundle.Description, userLinkBundle.Description);
            Assert.AreEqual(linkBundle.Links.Count, userLinkBundle.Links.Count);

            // Delete the recently created link bundle
            var deleteLinkBundleResponse = await this.client.DeleteAsync($"/api/links/{linkBundle.VanityUrl}");
            Assert.AreEqual(HttpStatusCode.NoContent, deleteLinkBundleResponse.StatusCode);

            // Ensure deleted bundle now returns 404
            var getLinkAfterDeleteResponse = await this.client.GetAsync($"/api/links/{linkBundle.VanityUrl}");
            Assert.AreEqual(HttpStatusCode.NotFound, getLinkAfterDeleteResponse.StatusCode);
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
