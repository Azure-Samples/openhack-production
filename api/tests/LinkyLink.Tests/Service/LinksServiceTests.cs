using Castle.Core.Configuration;
using LinkyLink.Helpers;
using LinkyLink.Models;
using LinkyLink.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace LinkyLink.Tests.Service
{
    /// <summary>
    /// Tests to validate methods in LinksService class.
    /// </summary>
    public class LinksServiceTests
    {
        private readonly Mock<LinksContext> _mockLinksContext;
        private readonly Mock<UserAuth> _mockUserAuth;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LinksService _linksService;

        public LinksServiceTests()
        {
            _httpContextAccessor = new HttpContextAccessor();
            _mockUserAuth = new Mock<UserAuth>(_httpContextAccessor);
            _mockLinksContext = new Mock<LinksContext>(new DbContextOptionsBuilder<LinksContext>().Options, new ConfigurationBuilder().Build());
            _linksService = new LinksService(_mockLinksContext.Object, _mockUserAuth.Object);
        }

        [Fact]
        public async Task CreateLinkBundleAsyncCreatesLinkBundleInDB()
        {
            // Arrange
            var linkBundles = new List<LinkBundle>
            {
                new LinkBundle { VanityUrl = "samplelink" }            
            };

            var MockSet = new Mock<DbSet<LinkBundle>>();
            MockSet.Setup(m => m.Add(It.IsAny<LinkBundle>())).Callback<LinkBundle>((entity) => linkBundles.Add(entity));
            _mockLinksContext.Setup(x => x.LinkBundle).Returns(MockSet.Object);

            await _linksService.CreateLinkBundleAsync(new LinkBundle
            {
                VanityUrl = "samplelink1"
            });

            Assert.Equal(2, linkBundles.Count);
        }

        [Fact]
        public async Task RemoveLinkBundleAsyncRemovesLinkBundleFromDB()
        {
            // Arrange
            var linkBundles = new List<LinkBundle>
            {
                new LinkBundle { Id = "samplelink" }
            };

            var MockSet = new Mock<DbSet<LinkBundle>>();
            MockSet.Setup(m => m.Remove(It.IsAny<LinkBundle>())).Callback<LinkBundle>((entity) => linkBundles.Remove(entity));
            _mockLinksContext.Setup(x => x.LinkBundle).Returns(MockSet.Object);

            await _linksService.RemoveLinkBundleAsync(linkBundles[0]);

            Assert.Equal(0, linkBundles.Count);
        }
    }
}
