using AutoFixture;
using FakeItEasy;
using LinkyLink.Controllers;
using LinkyLink.Models;
using LinkyLink.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace LinkyLink.Tests
{
    public class LinksControllerTests
    {
        private readonly Mock<ILinksService> _mockService;
        private readonly LinksController _linksController;

        public LinksControllerTests()
        {
            _mockService = new Mock<ILinksService>();
            _linksController = new LinksController(_mockService.Object);
        }

        [Fact]
        public async Task GetLinksReturnsNotFoundIfLinkBundleDoesntExists()
        {
            // Arrange 
            string vanityUrl = "DoesNotExists";
            
            // Act
            ActionResult<LinkBundle> result = await _linksController.GetLinkBundle(vanityUrl);
           
            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetLinksReturnsDocumentIfLinkBundleExists()
        {
            // Arrange 
            LinkBundle linkBundle = new LinkBundle
            {
                VanityUrl = "Exists"
            };

            _mockService.Setup(repo => repo.FindLinkBundle(linkBundle.VanityUrl))
                .ReturnsAsync(new LinkBundle() { VanityUrl = linkBundle.VanityUrl });

            // Act
            ActionResult<LinkBundle> result = await _linksController.GetLinkBundle(linkBundle.VanityUrl);

            // Assert
            Assert.IsType<LinkBundle>(result.Value);
        }
    }
}
