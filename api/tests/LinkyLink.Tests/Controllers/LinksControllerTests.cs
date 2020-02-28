using LinkyLink.Controllers;
using LinkyLink.Models;
using LinkyLink.Service;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Times = Moq.Times;

namespace LinkyLink.Tests
{
    /// <summary>
    /// Unit Tests to validate methods in LinksController class.
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
        public async Task GetLinkBundlesReturnsAllLinkBundles()
        {
            // Arrange
            _mockService.Setup(service => service.AllLinkBundlesAsync(QueryOptions.Default))
                .ReturnsAsync(It.IsAny<IEnumerable<LinkBundle>>);

            // Act
            var result = await _linksController.GetLinkBundlesAsync(QueryOptions.Default);

            // Assert
            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetLinkBundleReturnsNotFoundIfLinkBundleDoesntExist()
        {
            // Arrange 
            string vanityUrl = "samplelink";

            // Act
            ActionResult<LinkBundle> result = await _linksController.GetLinkBundleAsync(vanityUrl);

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

            _mockService.Setup(service => service.FindLinkBundleAsync(linkBundle.VanityUrl))
                .ReturnsAsync(linkBundle);

            // Act
            ActionResult<LinkBundle> result = await _linksController.GetLinkBundleAsync(linkBundle.VanityUrl);

            // Assert
            Assert.IsType<LinkBundle>(result.Value);
            Assert.Equal(result.Value.VanityUrl, linkBundle.VanityUrl);
        }

        [Fact]
        public async Task GetLinkBundlesForUserReturnsNotFoundIfLinkBundleDoesntExist()
        {
            // Arrange 
            string userId = "example@microsoft.com";

            _mockService.Setup(service => service.GetUserAccountEmail())
                .Returns(userId);

            // Act
            ActionResult<LinkBundle> result = await _linksController.GetLinkBundlesForUserAsync(userId, QueryOptions.Default);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetLinkBundlesForUserReturnsDocumentsIfLinkBundleExists()
        {
            // Arrange
            string userId = "example@microsoft.com";
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

            _mockService.Setup(service => service.GetUserAccountEmail())
                .Returns(userId);

            _mockService.Setup(service => service.FindLinkBundlesForUserAsync(userId, QueryOptions.Default))
                .ReturnsAsync(linkBundles);

            // Act
            ActionResult<LinkBundle> result = await _linksController.GetLinkBundlesForUserAsync(userId, QueryOptions.Default);

            // Assert
            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetLinkBundleForUserReturnsUnAuthorizedIfMissingAuth()
        {
            // Arrange
            LinkBundle linkBundle = new LinkBundle
            {
                UserId = "example@microsoft.com",
                VanityUrl = "samplelink"
            };

            // Act
            ActionResult<LinkBundle> result = await _linksController.GetLinkBundlesForUserAsync(linkBundle.UserId, QueryOptions.Default);

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
                UserId = "example@microsoft.com",
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

            _mockService.Setup(r => r.CreateLinkBundleAsync(It.IsAny<LinkBundle>()))
                .Callback<LinkBundle>(x => expectedLinkBundle = x);

            // Act
            ActionResult<LinkBundle> result = await _linksController.PostLinkBundleAsync(linkBundle);

            // Assert
            _mockService.Verify(x => x.CreateLinkBundleAsync(It.IsAny<LinkBundle>()), Times.Once);

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
                UserId = "example@microsoft.com",
                VanityUrl = string.Empty,
                Links = new List<Link> { new Link() }
            };

            _mockService.Setup(r => r.CreateLinkBundleAsync(It.IsAny<LinkBundle>()))
                .Callback<LinkBundle>(x => expectedLinkBundle = x);

            // Act
            ActionResult<LinkBundle> result = await _linksController.PostLinkBundleAsync(linkBundle);

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
                UserId = "example@microsoft.com",
                VanityUrl = vanityUrl,
                Links = new List<Link> { new Link() }
            };

            _mockService.Setup(r => r.CreateLinkBundleAsync(It.IsAny<LinkBundle>()))
                .Callback<LinkBundle>(x => expectedLinkBundle = x);

            // Act
            ActionResult<LinkBundle> result = await _linksController.PostLinkBundleAsync(linkBundle);

            // Assert
            Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(vanityUrl.ToLower(), expectedLinkBundle.VanityUrl);
        }

        [Fact]
        public async Task PostLinkBundleReturnsBadRequestIfVanityUrlNameIsInvalid()
        {
            // Arrange
            LinkBundle linkBundle = new LinkBundle
            {
                UserId = "example@microsoft.com",
                VanityUrl = "name@",
                Links = new List<Link> { new Link() }
            };

            // Act
            ActionResult<LinkBundle> result = await _linksController.PostLinkBundleAsync(linkBundle);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task PostLinkBundleReturnsBadRequestIfLinksAreNotProvided()
        {
            // Arrange
            LinkBundle linkBundle = new LinkBundle
            {
                UserId = "example@microsoft.com",
                VanityUrl = string.Empty
            };

            // Act
            ActionResult<LinkBundle> result = await _linksController.PostLinkBundleAsync(linkBundle);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task PostLinkBundleReturnsConflictIfRecordExists()
        {
            // Arrange
            LinkBundle linkBundle = new LinkBundle
            {
                UserId = "example@microsoft.com",
                VanityUrl = string.Empty,
                Links = new List<Link> { new Link() }
            };

            _mockService.Setup(service => service.LinkBundleExistsAsync(linkBundle.Id))
                .ReturnsAsync(true);

            _mockService.Setup(r => r.CreateLinkBundleAsync(linkBundle))
                .Throws(new DbUpdateException());

            // Act
            ActionResult<LinkBundle> result = await _linksController.PostLinkBundleAsync(linkBundle);

            // Assert
            Assert.IsType<ConflictResult>(result.Result);
        }

        [Fact]
        public async Task PostLinkBundleThrowsDBUpdateException()
        {
            // Arrange
            LinkBundle linkBundle = new LinkBundle
            {
                UserId = "example@microsoft.com",
                VanityUrl = string.Empty,
                Links = new List<Link> { new Link() }
            };

            _mockService.Setup(r => r.CreateLinkBundleAsync(linkBundle))
                .Throws(new DbUpdateException());

            // Act, Assert
            Assert.ThrowsAsync<DbUpdateException>(() => _linksController.PostLinkBundleAsync(linkBundle));
        }

        [Fact]
        public async Task DeleteLinkBundleReturnsUnAuthorizedIfMissingAuth()
        {
            // Arrange
            LinkBundle linkBundle = new LinkBundle
            {
                UserId = "example@microsoft.com",
                VanityUrl = "samplelink"
            };

            // Act
            ActionResult<LinkBundle> result = await _linksController.DeleteLinkBundleAsync(linkBundle.VanityUrl);

            // Assert
            Assert.IsType<UnauthorizedResult>(result.Result);
        }

        [Fact]
        public async Task DeleteLinkBundleReturnsNotFoundIfLinkBundleDoesntExist()
        {
            // Arrange 
            string userId = "example@microsoft.com";
            string vanityUrl = "sampleVanityUrl";

            _mockService.Setup(service => service.GetUserAccountEmail())
                .Returns("example@microsoft.com");

            // Act
            ActionResult<LinkBundle> result = await _linksController.DeleteLinkBundleAsync(vanityUrl);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task DeleteLinkBundleAllowsOtherOwnerToDeleteBundles()
        {
            // Arrange 
            LinkBundle linkBundle = new LinkBundle
            {
                UserId = "example1@microsoft.com",
                VanityUrl = "samplelink"
            };

            _mockService.Setup(service => service.GetUserAccountEmail())
                .Returns("example@microsoft.com");

            _mockService.Setup(service => service.FindLinkBundleAsync(linkBundle.VanityUrl))
                .ReturnsAsync(linkBundle);

            // Act
            ActionResult<LinkBundle> result = await _linksController.DeleteLinkBundleAsync(linkBundle.VanityUrl);

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
            ActionResult<LinkBundle> result = await _linksController.PatchLinkBundleAsync(vanityUrl, patchReqDocument);

            // Assert
            Assert.IsType<UnauthorizedResult>(result.Result);
        }

        [Fact]
        public async Task PatchLinkBundleReturnsNotFoundIfLinkBundleDoesntExist()
        {
            // Arrange
            LinkBundle linkBundle = new LinkBundle
            {
                UserId = "example@microsoft.com",
                VanityUrl = "samplelink"
            };

            _mockService.Setup(service => service.GetUserAccountEmail())
                .Returns("example@microsoft.com");

            JsonPatchDocument<LinkBundle> patchReqDocument = new JsonPatchDocument<LinkBundle>();

            // Act
            ActionResult<LinkBundle> result = await _linksController.PatchLinkBundleAsync(linkBundle.VanityUrl, patchReqDocument);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task PatchLinkBundleReturnsForbiddenIfLinkBundleOwnedByOtherUser()
        {
            // Arrange 
            LinkBundle linkBundle = new LinkBundle
            {
                UserId = "example1@microsoft.com",
                VanityUrl = "samplelink"
            };

            _mockService.Setup(service => service.GetUserAccountEmail())
                .Returns("example@microsoft.com");

            _mockService.Setup(service => service.FindLinkBundleAsync(linkBundle.VanityUrl))
                .ReturnsAsync(linkBundle);

            JsonPatchDocument<LinkBundle> patchReqDocument = new JsonPatchDocument<LinkBundle>();

            // Act
            ActionResult<LinkBundle> result = await _linksController.PatchLinkBundleAsync(linkBundle.VanityUrl, patchReqDocument);

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
                UserId = "example@microsoft.com",
                VanityUrl = "samplelink"
            };

            _mockService.Setup(service => service.GetUserAccountEmail())
                .Returns(linkBundle.UserId);

            _mockService.Setup(service => service.FindLinkBundleAsync(linkBundle.VanityUrl))
                .ReturnsAsync(linkBundle);

            _mockService.Setup(r => r.UpdateLinkBundleAsync(It.IsAny<LinkBundle>()))
                .Callback<LinkBundle>(x => expectedLinkBundle = x);

            string description = "sampledescription";
            JsonPatchDocument<LinkBundle> patchReqDocument = new JsonPatchDocument<LinkBundle>();
            patchReqDocument.Add(d => d.Description, description);

            // Act
            ActionResult<LinkBundle> result = await _linksController.PatchLinkBundleAsync(linkBundle.VanityUrl, patchReqDocument);

            // Assert
            Assert.IsType<NoContentResult>(result.Result);
            Assert.Equal(linkBundle.VanityUrl, expectedLinkBundle.VanityUrl);
            Assert.Equal(linkBundle.UserId, expectedLinkBundle.UserId);
            Assert.Equal(description, expectedLinkBundle.Description);
        }
    }
}
