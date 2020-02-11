using AutoFixture;
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
using System.Threading.Tasks;
using Xunit;

namespace LinkyLink.Tests.Service
{
    public class LinksServiceTests
    {
        private readonly Mock<LinksContext> _mockLinksContext;
        private readonly UserAuth _userAuth;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private LinksService _linksService;

        public LinksServiceTests()
        {
            _httpContextAccessor = new HttpContextAccessor();
            _userAuth = new UserAuth(_httpContextAccessor);
            _mockLinksContext = new Mock<LinksContext>(new DbContextOptionsBuilder<LinksContext>().Options, new ConfigurationBuilder().Build());
            _linksService = new LinksService(_mockLinksContext.Object, _userAuth);
        }

        [Fact]
        public async Task CreateLinkBundleAsyncCreatesLinkBundle()
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
        public async Task RemoveLinkBundleAsyncRemovesLinkBundle()
        {
            // Arrange
            var linkBundles = new List<LinkBundle>
            {
                new LinkBundle { Id = "samplelink" }
            };

            var MockSet = new Mock<DbSet<LinkBundle>>();
            MockSet.Setup(m => m.Remove(It.IsAny<LinkBundle>())).Callback<LinkBundle>((entity) => linkBundles.Remove(entity));
            _mockLinksContext.Setup(x => x.LinkBundle).Returns(MockSet.Object);

            //_mockLinksContext.Setup(m => m.LinkBundle.Remove(It.IsAny<LinkBundle>())).Callback<LinkBundle>((entity) => linkBundles.Remove(entity));

            await _linksService.RemoveLinkBundleAsync(linkBundles[0]);

            Assert.Equal(0, linkBundles.Count);
        }

        //[Fact]
        //public async Task AllLinkBundlesAsync()
        //{
        //    // Arrange
        //    var linkBundles = new List<LinkBundle>
        //    {
        //        new LinkBundle { Id = "samplelink" }
        //    }.AsQueryable();

        //    var usersMock = new Mock<DbSet<LinkBundle>>();
        //    usersMock.As<IQueryable<LinkBundle>>().Setup(m => m.Provider).Returns(linkBundles.Provider);
        //    usersMock.As<IQueryable<LinkBundle>>().Setup(m => m.Expression).Returns(linkBundles.Expression);
        //    usersMock.As<IQueryable<LinkBundle>>().Setup(m => m.ElementType).Returns(linkBundles.ElementType);
        //    usersMock.As<IQueryable<LinkBundle>>().Setup(m => m.GetEnumerator()).Returns(linkBundles.GetEnumerator());

        //    _mockLinksContext.Setup(x => x.LinkBundle).Returns(usersMock.Object);

        //    var link = await _linksService.AllLinkBundlesAsync();

        //    Assert.Equal(1, link.Count());
        //}

        //[Fact]
        //public async Task UpdateLinkBundleAsync()
        //{
        //    // Arrange
        //    var linkBundles = new List<LinkBundle>
        //    {
        //        new LinkBundle { Id = "samplelink" }
        //    };

        //    var MockSet = new Mock<DbSet<LinkBundle>>();
        //    //MockSet.Setup(m => m.Update(It.IsAny<LinkBundle>())).Callback<LinkBundle>((entity) => linkBundles.(entity));
        //    _mockLinksContext.Setup(x => x.Entry(It.IsAny<LinkBundle>())).Returns(() => _mockLinksContext.Object.Entry(linkBundles[0]));

        //    //_mockLinksContext.Setup(m => m.LinkBundle.Remove(It.IsAny<LinkBundle>())).Callback<LinkBundle>((entity) => linkBundles.Remove(entity));

        //    await _linksService.UpdateLinkBundleAsync(new LinkBundle { Id = "samplelink", VanityUrl = "12" });

        //    Assert.Equal(0, linkBundles.Count);
        //}
    }
}
