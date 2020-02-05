using LinkyLink.Controllers;
using LinkyLink.Models;
using LinkyLink.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace LinkyLink.Tests
{
    /// <summary>
    /// Tests to validate methods in OpenGraphController class.
    /// </summary>
    public class OpenGraphControllerTests
    {
        private readonly OpenGraphController _openGraphController;
        private readonly Mock<IOpenGraphService> _mockService;

        public OpenGraphControllerTests()
        {
            _mockService = new Mock<IOpenGraphService>();
            _openGraphController = new OpenGraphController(_mockService.Object);
        }

        [Fact]
        public async Task OpenGraphReturnsBadRequestWhenEmptyPayload()
        {
            // Arrange
            List<OpenGraphRequest> openGraphRequests = null;

            // Act
            ActionResult<OpenGraphResult> result = await _openGraphController.PostAsync(openGraphRequests);

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
