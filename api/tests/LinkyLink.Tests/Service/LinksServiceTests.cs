using Castle.Core.Configuration;
using LinkyLink.Helpers;
using LinkyLink.Models;
using LinkyLink.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MockQueryable.Moq;
using Moq;
using System;
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
        private readonly List<LinkBundle> _sourceList;
        private readonly Mock<DbSet<LinkBundle>> _mockSourceList;

        public LinksServiceTests()
        {
            _sourceList = new List<LinkBundle>();
            _mockSourceList = _sourceList.AsQueryable().BuildMockDbSet();
            _httpContextAccessor = new HttpContextAccessor();
            _mockUserAuth = new Mock<UserAuth>(_httpContextAccessor);
            _mockLinksContext = new Mock<LinksContext>(new DbContextOptionsBuilder<LinksContext>().Options, new ConfigurationBuilder().Build());
            _mockLinksContext.Setup(m => m.LinkBundle).Returns(_mockSourceList.Object);
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

        [Fact]
        public async Task GetAllLinkBundlesWithPaging()
        {
            for (var i = 1; i <= 100; i++)
            {
                _sourceList.Add(new LinkBundle
                {
                    Id = Guid.NewGuid().ToString(),
                    Description = $"Link Bundle {i}",
                    VanityUrl = $"link-bundle-{i}"
                });
            }

            var queryOptions = new QueryOptions
            {
                Skip = 10,
                Top = 10
            };

            var results = (await _linksService.AllLinkBundlesAsync(queryOptions)).ToList();
            Assert.Equal(queryOptions.Top, results.Count());
            Assert.Equal("Link Bundle 11", results[0].Description);
        }
    }
}
