using LinkyLink.Helpers;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using Xunit;

namespace LinkyLink.Tests.Helpers
{
    public class AppInsightsTelemetryInitializerTests
    {
        ITelemetryInitializer telemetryInitializer;

        public AppInsightsTelemetryInitializerTests()
        {
            var mockHttpContextAccessory = new Mock<IHttpContextAccessor>();
            var defaultContext = new DefaultHttpContext();

            var identity = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "username")
            }, "custom");

            var principal = new ClaimsPrincipal(identity);

            defaultContext.User = principal;

            mockHttpContextAccessory
                .Setup(m => m.HttpContext)
                .Returns(defaultContext);

            this.telemetryInitializer = new AppInsightsTelemetryInitializer(mockHttpContextAccessory.Object);
        }

        [Fact]
        public void InitializerSetsRoleName()
        {
            var requestTelemetry = new RequestTelemetry();
            this.telemetryInitializer.Initialize(requestTelemetry);

            Assert.Equal("API", requestTelemetry.Context.Cloud.RoleName);
            Assert.Equal(Environment.MachineName, requestTelemetry.Context.Cloud.RoleInstance);
        }

        [Fact]
        public void InitializerLogsIsAuthenticatedTrue()
        {
            var requestTelemetry = new RequestTelemetry();
            this.telemetryInitializer.Initialize(requestTelemetry);
            Assert.Equal("True", requestTelemetry.Properties["IsAuthenticated"]);
        }

        [Fact]
        public void InitializerLogsIsAuthenticatedFalse()
        {
            var mockHttpContextAccessory = new Mock<IHttpContextAccessor>();
            mockHttpContextAccessory
                .Setup(m => m.HttpContext)
                .Returns(new DefaultHttpContext());

            var defaultContext = new DefaultHttpContext();
            var telemetryInitializer = new AppInsightsTelemetryInitializer(mockHttpContextAccessory.Object);

            var requestTelemetry = new RequestTelemetry();
            telemetryInitializer.Initialize(requestTelemetry);
            Assert.Equal("False", requestTelemetry.Properties["IsAuthenticated"]);
        }
    }
}
