using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using System;

namespace LinkyLink.Helpers
{
    public class AppInsightsTelemetryInitializer : ITelemetryInitializer
    {
        private readonly HttpContext httpContext;

        public AppInsightsTelemetryInitializer(IHttpContextAccessor contextAccessor)
        {
            this.httpContext = contextAccessor.HttpContext;
        }

        public void Initialize(ITelemetry telemetry)
        {
            // Setup cloud role name used in application map
            // https://docs.microsoft.com/en-us/azure/azure-monitor/app/app-map#set-cloud-role-name
            telemetry.Context.Cloud.RoleName = "API";
            telemetry.Context.Cloud.RoleInstance = Environment.MachineName;

            if (httpContext != null)
            {
                var requestTelemetry = telemetry as RequestTelemetry;
                if (requestTelemetry != null)
                {
                    // TODO: We should figure out how to track requests against the current logged in user.
                    requestTelemetry.Properties["IsAuthenticated"] = this.httpContext.User.Identity.IsAuthenticated.ToString();
                }
            }
        }
    }
}
