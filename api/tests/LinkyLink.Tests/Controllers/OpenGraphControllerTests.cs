using LinkyLink.Controllers;
using LinkyLink.Models;
using LinkyLink.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
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
        public async Task PostAsyncReturnsBadRequestWhenEmptyPayload()
        {
            // Arrange
            List<OpenGraphRequest> openGraphRequests = null;

            // Act
            ActionResult<OpenGraphResult> result = await _openGraphController.PostAsync(openGraphRequests);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task PostAsyncThrowsExceptionWhenOpenGraphAPIFails()
        {
            // Arrange
            List<OpenGraphRequest> openGraphRequests = new List<OpenGraphRequest>();

            OpenGraphRequest openGraphRequest = new OpenGraphRequest
            {
                Url = "www.microsoft.com",
                Id = "1"
            };

            openGraphRequests.Add(openGraphRequest);

            // Act, // Assert
            Assert.ThrowsAsync<Exception>(() => _openGraphController.PostAsync(openGraphRequests));
        }

        [Fact]
        public async Task PostAsyncReturnsOpenGraphAPI()
        {
            // Arrange
            List<OpenGraphRequest> openGraphRequests = new List<OpenGraphRequest>()
            {
                new OpenGraphRequest
                {
                    Url = "www.microsoft.com",
                
                    Id = "1"
                }
            };
     
            IEnumerable<OpenGraphResult> resultList = new List<OpenGraphResult>()
            {
                new OpenGraphResult
                {
                    Id = "1",
                    Title = "Microsoft",
                    Description = "Microsoft"
                }
            };

            Mock<HttpRequest> request = new Mock<HttpRequest>();
            _mockService.Setup(x => x.GetGraphResultsAsync(request.Object, openGraphRequests)).Returns(Task.FromResult(resultList));

            // Act
            ActionResult<OpenGraphResult> result = await _openGraphController.PostAsync(openGraphRequests);

            // Assert
            Assert.IsType<OkObjectResult>(result.Result);
        }
    }
}
