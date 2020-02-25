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
            _mockSourceList
                .Setup(m => m.Add(It.IsAny<LinkBundle>()))
                .Callback<LinkBundle>(bundle => _sourceList.Add(bundle));

            _mockSourceList
                .Setup(m => m.Remove(It.IsAny<LinkBundle>()))
                .Callback<LinkBundle>(bundle => _sourceList.Remove(bundle));

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
            var newBundle = new LinkBundle { VanityUrl = "samplelink" };
            Assert.Equal(0, _sourceList.Count());

            // Act
            await _linksService.CreateLinkBundleAsync(newBundle);

            // Assert
            Assert.Equal(1, _sourceList.Count());
        }

        [Fact]
        public async Task RemoveLinkBundleAsyncRemovesLinkBundleFromDB()
        {
            // Arrange
            var bundleToDelete = new LinkBundle { Id = "samplelink" };
            _sourceList.Add(bundleToDelete);
            Assert.Equal(1, _sourceList.Count);

            // Act
            await _linksService.RemoveLinkBundleAsync(bundleToDelete);


            //Asert
            Assert.Equal(0, _sourceList.Count);
        }

        [Fact]
        public async Task GetAllLinkBundlesWithExplicitPaging()
        {
            // Arrange
            for (var i = 1; i <= 100; i++)
            {
                _sourceList.Add(new LinkBundle
                {
                    Id = Guid.NewGuid().ToString(),
                    Description = $"Link Bundle {i}",
                    VanityUrl = $"link-bundle-{i}"
                });
            }

            // Act
            var queryOptions = new QueryOptions
            {
                Skip = 10,
                Top = 10
            };

            var results = (await _linksService.AllLinkBundlesAsync(queryOptions)).ToList();

            // Assert
            Assert.Equal(queryOptions.Top, results.Count());
            Assert.Equal("Link Bundle 11", results[0].Description);
        }

        [Fact]
        public async Task GetAllLinkBundlesWithDefaultPaging()
        {
            // Arrange
            for (var i = 1; i <= 100; i++)
            {
                _sourceList.Add(new LinkBundle
                {
                    Id = Guid.NewGuid().ToString(),
                    Description = $"Link Bundle {i}",
                    VanityUrl = $"link-bundle-{i}"
                });
            }

            // Act
            var results = (await _linksService.AllLinkBundlesAsync()).ToList();

            // Assert
            Assert.Equal(QueryOptions.Default.Top, results.Count());
            Assert.Equal("Link Bundle 1", results[0].Description);
        }

        [Fact]
        public async Task FindLinkBundleWithMatchingVanityUrl()
        {
            // Arrange
            for (var i = 1; i <= 100; i++)
            {
                _sourceList.Add(new LinkBundle
                {
                    Id = Guid.NewGuid().ToString(),
                    Description = $"Link Bundle {i}",
                    VanityUrl = $"link-bundle-{i}"
                });
            }

            var random = new Random(DateTime.UtcNow.Millisecond);
            var index = random.Next(0, 99);
            var vanityUrl = _sourceList[index].VanityUrl;

            // Act
            var match = await _linksService.FindLinkBundleAsync(vanityUrl);

            // Assert
            Assert.NotNull(match);
            Assert.Equal(vanityUrl, match.VanityUrl);
        }

        [Fact]
        public async Task FindLinkBundleWithoutMatchingVanityUrl()
        {
            // Arrage

            // Act
            var match = await _linksService.FindLinkBundleAsync(Guid.NewGuid().ToString());

            // Assert
            Assert.Null(match);
        }

        [Fact]
        public async Task FindLinkBundleExistsReturnsTrue()
        {
            // Arrange
            for (var i = 1; i <= 100; i++)
            {
                _sourceList.Add(new LinkBundle
                {
                    Id = Guid.NewGuid().ToString(),
                    Description = $"Link Bundle {i}",
                    VanityUrl = $"link-bundle-{i}"
                });
            }

            var random = new Random(DateTime.UtcNow.Millisecond);
            var index = random.Next(0, 99);

            // Act
            var exists = await _linksService.LinkBundleExistsAsync(_sourceList[index].Id);

            // Assert
            Assert.True(exists);
        }

        [Fact]
        public async Task FindLinkBundleExistsReturnsFalse()
        {
            // Arrange

            // Act
            var exists = await _linksService.LinkBundleExistsAsync(Guid.NewGuid().ToString());

            // Assert
            Assert.False(exists);
        }

        [Fact]
        public async Task FindLinkBundlesForUserWithExplicitPaging()
        {
            // Arrange
            var userId = "user@contoso.com";

            for (var i = 1; i <= 100; i++)
            {
                _sourceList.Add(new LinkBundle
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    Description = $"Link Bundle {i}",
                    VanityUrl = $"link-bundle-{i}"
                });
            }

            // Act
            var queryOptions = new QueryOptions
            {
                Skip = 10,
                Top = 10
            };

            var results = (await _linksService.FindLinkBundlesForUserAsync(userId, queryOptions)).ToList();

            // Assert
            Assert.Equal(queryOptions.Top, results.Count());
            Assert.Equal("Link Bundle 11", results[0].Description);
        }

        [Fact]
        public async Task FindLinkBundlesForUserWithDefaultPaging()
        {
            // Arrange
            var userId = "user@contoso.com";
            for (var i = 1; i <= 100; i++)
            {
                _sourceList.Add(new LinkBundle
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    Description = $"Link Bundle {i}",
                    VanityUrl = $"link-bundle-{i}"
                });
            }

            // Act
            var results = (await _linksService.FindLinkBundlesForUserAsync(userId)).ToList();

            // Assert
            Assert.Equal(QueryOptions.Default.Top, results.Count());
            Assert.Equal("Link Bundle 1", results[0].Description);
        }
    }
}
