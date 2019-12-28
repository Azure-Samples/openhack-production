using LinkyLink.Controllers;
using LinkyLink.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

        public ValidatePageControllerTests()
        {
            _validatePageController = new ValidatePageController();
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
