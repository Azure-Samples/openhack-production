using LinkyLink.Controllers;
using LinkyLink.Models;
using LinkyLink.Service;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Times = Moq.Times;

namespace LinkyLink.Tests
{
    /// <summary>
    /// Tests to validate methods in LinksController class.
    /// </summary>
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
        public async Task GetLinkBundleReturnsNotFoundIfLinkBundleDoesntExists()
        {
            // Arrange 
            string vanityUrl = "samplelink";
            
            // Act
            ActionResult<LinkBundle> result = await _linksController.GetLinkBundle(vanityUrl);
           
            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetLinkBundleReturnsDocumentIfLinkBundleExists()
        {
            // Arrange 
            LinkBundle linkBundle = new LinkBundle
            {
                VanityUrl = "samplelink"
            };

            _mockService.Setup(service => service.FindLinkBundle(linkBundle.VanityUrl))
                .ReturnsAsync(linkBundle);

            // Act
            ActionResult<LinkBundle> result = await _linksController.GetLinkBundle(linkBundle.VanityUrl);

            // Assert
            Assert.IsType<LinkBundle>(result.Value);
            Assert.Equal(result.Value.VanityUrl, linkBundle.VanityUrl);
        }

        [Fact]
        public async Task GetLinkBundlesForUserReturnsNotFoundIfLinkBundleDoesntExists()
        {
            // Arrange 
            string userId = "userhash";

            _mockService.Setup(service => service.GetUserAccountHash())
                .Returns(userId);

            // Act
            ActionResult<LinkBundle> result = await _linksController.GetLinkBundlesForUser(userId);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetLinkBundlesForUserReturnsDocumentsIfLinkBundleExists()
        {
            // Arrange
            string userId = "userhash";
            List<LinkBundle> linkBundles = new List<LinkBundle>
            {
                new LinkBundle
                {
                    UserId = userId,
                    VanityUrl = "samplelink",
                    Links = new List<Link>
                    {
                        new Link
                        {
                            Id = "sample"
                        }
                    }
                },                
                new LinkBundle
                {
                    UserId = userId,
                    VanityUrl = "samplelink1",      
                    Links = new List<Link>
                    {
                        new Link
                        {
                            Id = "sample1"
                        }
                    }
                }
            };
            
            _mockService.Setup(service => service.GetUserAccountHash())
                .Returns(userId);

            _mockService.Setup(service => service.FindLinkBundlesForUser(userId))
                .ReturnsAsync(linkBundles);

            // Act
            ActionResult<LinkBundle> result = await _linksController.GetLinkBundlesForUser(userId);

            // Assert
            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetLinkBundleForUserReturnsUnAuthorizedIfMissingAuth()
        {
            // Arrange
            LinkBundle linkBundle = new LinkBundle
            {
                UserId = "userhash",
                VanityUrl = "samplelink"
            };

            // Act
            ActionResult<LinkBundle> result = await _linksController.GetLinkBundlesForUser(linkBundle.UserId);

            // Assert
            Assert.IsType<UnauthorizedResult>(result.Result);
        }

        [Fact]
        public async Task PostLinkBundleCreatesLinkBundleWhenValidPayload()
        {
            // Arrange
            LinkBundle expectedLinkBundle = null;
            
            LinkBundle linkBundle = new LinkBundle
            {
                UserId = "userhash",
                VanityUrl = "samplelink",
                Description = "sampledescription",
                Links = new List<Link>    
                {    
                    new Link    
                    { 
                        Id = "sample"
                    }
                }
            };

            _mockService.Setup(r => r.CreateLinkBundle(It.IsAny<LinkBundle>()))
                .Callback<LinkBundle>(x => expectedLinkBundle = x);

            // Act
            ActionResult<LinkBundle> result = await _linksController.PostLinkBundle(linkBundle);

            // Assert
            _mockService.Verify(x => x.CreateLinkBundle(It.IsAny<LinkBundle>()), Times.Once);
            
            Assert.IsType<CreatedAtActionResult>(result.Result);
            
            Assert.Equal(linkBundle.Description, expectedLinkBundle.Description);
            Assert.Equal(linkBundle.UserId, expectedLinkBundle.UserId);
            Assert.Equal(linkBundle.VanityUrl, expectedLinkBundle.VanityUrl);
            Assert.Equal(linkBundle.Links.Count, expectedLinkBundle.Links.Count());
        }

        [Fact]
        public async Task PostLinkBundlePopulatesVanityUrlIfNotProvided()
        {
            // Arrange
            LinkBundle expectedLinkBundle = null;

            LinkBundle linkBundle = new LinkBundle
            {
                UserId = "userhash",
                VanityUrl = string.Empty,
                Links = new List<Link> { new Link() }
            };

            _mockService.Setup(r => r.CreateLinkBundle(It.IsAny<LinkBundle>()))
                .Callback<LinkBundle>(x => expectedLinkBundle = x);

            // Act
            ActionResult<LinkBundle> result = await _linksController.PostLinkBundle(linkBundle);

            // Assert
            Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.False(string.IsNullOrEmpty(expectedLinkBundle.VanityUrl));
        }

        [Theory]
        [InlineData("lower")]
        [InlineData("UPPER")]
        [InlineData("MiXEd")]
        public async Task PostLinkBundleConvertsVanityUrlToLowerCase(string vanityUrl)
        {
            // Arrange
            LinkBundle expectedLinkBundle = null;

            LinkBundle linkBundle = new LinkBundle
            {
                UserId = "userhash",
                VanityUrl = vanityUrl,
                Links = new List<Link> { new Link() }
            };

            _mockService.Setup(r => r.CreateLinkBundle(It.IsAny<LinkBundle>()))
                .Callback<LinkBundle>(x => expectedLinkBundle = x);

            // Act
            ActionResult<LinkBundle> result = await _linksController.PostLinkBundle(linkBundle);

            // Assert
            Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(vanityUrl.ToLower(), expectedLinkBundle.VanityUrl);
        }

        [Fact]
        public async Task DeleteLinkBundleReturnsUnAuthorizedIfMissingAuth()
        {
            // Arrange
            LinkBundle linkBundle = new LinkBundle
            {
                UserId = "userhash",
                VanityUrl = "samplelink"
            };

            // Act
            ActionResult<LinkBundle> result = await _linksController.DeleteLinkBundle(linkBundle.VanityUrl);

            // Assert
            Assert.IsType<UnauthorizedResult>(result.Result);
        }

        [Fact]
        public async Task DeleteLinkBundleReturnsNotFoundIfLinkBundleDoesntExists()
        {
            // Arrange 
            string userId = "userhash";
            string vanityUrl = "sampleVanityUrl";

            _mockService.Setup(service => service.GetUserAccountHash())
                .Returns("userhash");

            // Act
            ActionResult<LinkBundle> result = await _linksController.DeleteLinkBundle(vanityUrl);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task DeleteLinkBundleAllowsOtherOwnerToDeleteBundles()
        {
            // Arrange 
            LinkBundle linkBundle = new LinkBundle
            {
                UserId = "userhash1",
                VanityUrl = "samplelink"
            };

            _mockService.Setup(service => service.GetUserAccountHash())
                .Returns("userhash");

            _mockService.Setup(service => service.FindLinkBundle(linkBundle.VanityUrl))
                .ReturnsAsync(linkBundle);

            // Act
            ActionResult<LinkBundle> result = await _linksController.DeleteLinkBundle(linkBundle.VanityUrl);

            // Assert
            Assert.IsType<NoContentResult>(result.Result);
        }

        [Fact]
        public async Task PatchLinkBundleReturnsUnAuthorizedIfMissingAuth()
        {
            // Arrange
            string vanityUrl = "samplelink";

            JsonPatchDocument<LinkBundle> patchReqDocument = new JsonPatchDocument<LinkBundle>();

            // Act
            ActionResult<LinkBundle> result = await _linksController.PatchLinkBundle(vanityUrl, patchReqDocument);

            // Assert
            Assert.IsType<UnauthorizedResult>(result.Result);
        }

        [Fact]
        public async Task PatchLinkBundleReturnsForbiddenIfLinkBundleOwnedByOtherUser()
        {
            // Arrange 
            LinkBundle linkBundle = new LinkBundle
            {
                UserId = "userhash1",
                VanityUrl = "samplelink"
            };

            _mockService.Setup(service => service.GetUserAccountHash())
                .Returns("userhash");

            _mockService.Setup(service => service.FindLinkBundle(linkBundle.VanityUrl))
                .ReturnsAsync(linkBundle);

            JsonPatchDocument<LinkBundle> patchReqDocument = new JsonPatchDocument<LinkBundle>();

            // Act
            ActionResult<LinkBundle> result = await _linksController.PatchLinkBundle(linkBundle.VanityUrl, patchReqDocument);

            // Assert
            Assert.IsType<ForbidResult>(result.Result);
        }

        [Fact]
        public async Task PatchLinkBundleAppliesJsonPathToLinkBundle()
        {
            // Arrange 
            LinkBundle expectedLinkBundle = null;

            LinkBundle linkBundle = new LinkBundle
            {
                UserId = "userhash",
                VanityUrl = "samplelink"
            };

            _mockService.Setup(service => service.GetUserAccountHash())
                .Returns(linkBundle.UserId);

            _mockService.Setup(service => service.FindLinkBundle(linkBundle.VanityUrl))
                .ReturnsAsync(linkBundle);

            _mockService.Setup(r => r.UpdateLinkBundle(It.IsAny<LinkBundle>()))
                .Callback<LinkBundle>(x => expectedLinkBundle = x);

            string description = "sampledescription";
            JsonPatchDocument<LinkBundle> patchReqDocument = new JsonPatchDocument<LinkBundle>();
            patchReqDocument.Add(d => d.Description, description);

            // Act
            ActionResult<LinkBundle> result = await _linksController.PatchLinkBundle(linkBundle.VanityUrl, patchReqDocument);

            // Assert
            Assert.IsType<NoContentResult>(result.Result);
            Assert.Equal(linkBundle.VanityUrl, expectedLinkBundle.VanityUrl);
            Assert.Equal(linkBundle.UserId, expectedLinkBundle.UserId);
            Assert.Equal(description, expectedLinkBundle.Description);
        }
    }
}
