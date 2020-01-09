using LinkyLink.Controllers;
using LinkyLink.Models;
using LinkyLink.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace LinkyLink.Tests
{
    /// <summary>
    /// Tests to validate methods in ValidatePageController class.
    /// </summary>
    public class ValidatePageControllerTests
    {
        private readonly ValidatePageController _validatePageController;
        private readonly Mock<IOpenGraphService> _mockService;

        public ValidatePageControllerTests()
        {
            _mockService = new Mock<IOpenGraphService>();
            _validatePageController = new ValidatePageController(_mockService.Object);
        }

        [Fact]
        public async Task ValidatePageReturnsBadRequestWhenEmptyPayload()
        {
            // Arrange, Act
            ActionResult<OpenGraphResult> result = await _validatePageController.Post();

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);

            BadRequestObjectResult badRequestResult = result.Result as BadRequestObjectResult;
            Assert.IsType<ProblemDetails>(badRequestResult.Value);

            ProblemDetails problemDetails = badRequestResult.Value as ProblemDetails;
            Assert.Equal("Could not validate links", problemDetails.Title);
            Assert.Equal(problemDetails.Status, StatusCodes.Status400BadRequest);
        }
    }
}
