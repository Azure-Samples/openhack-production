using LinkyLink.Controllers;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace LinkyLink.Tests
{
    /// <summary>
    /// Tests to validate methods in HealthController class.
    /// </summary>
    public class HealthControllerTests
    {
        private readonly HealthController _healthController;

        public HealthControllerTests()
        {
            _healthController = new HealthController();
        }

        [Fact]
        public void PingReturnsOk()
        {
            // Arrange, Act
            ActionResult result = _healthController.Ping();

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }
    }
}
