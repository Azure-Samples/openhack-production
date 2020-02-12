using LinkyLink.Models;
using LinkyLink.Service;
using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace LinkyLink.Tests.Service
{
    public class OpenGraphServiceTests
    {
        public readonly OpenGraphService _openGraphService;
        public readonly Mock<HttpRequest> _mockHttpRequest;

        public OpenGraphServiceTests()
        {
            _openGraphService = new OpenGraphService();
            _mockHttpRequest = new Mock<HttpRequest>();
        }

        [Fact]
        public async Task GetGraphResultsAsyncReturnsOpenGraphAPIResults()
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

            // Act
            IEnumerable<OpenGraphResult> openGraphResult = await _openGraphService.GetGraphResultsAsync(_mockHttpRequest.Object, openGraphRequests);

            // Assert
            var enumerator = openGraphResult.GetEnumerator();
            enumerator.MoveNext();
            OpenGraphResult firstElement = enumerator.Current;

            Assert.NotNull(firstElement.Id);
            Assert.NotNull(firstElement.Title);
            Assert.NotNull(firstElement.Description);
        }
    }
}
