using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using LinkyLink.Helpers;
using LinkyLink.Models;
using LinkyLink.Service;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureADB2C.UI;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace LinkyLink
{
    [ExcludeFromCodeCoverage]
    public class Startup
    {
        readonly string _CORSPolicyName = "OHCORSPolicyName";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            IConfigurationSection configSection = Configuration.GetSection("CosmosSettings");
            services.AddDbContext<LinksContext>(options => options.UseCosmos(configSection["ServiceEndpoint"], configSection["AuthKey"], configSection["DatabaseName"]));
            services.AddControllers().AddNewtonsoftJson();
            services.AddHttpContextAccessor();
            services.AddTransient<ILinksService, LinksService>();
            services.AddTransient<IOpenGraphService, OpenGraphService>();
            services.AddSingleton<UserAuth>();

            // Add CORS policy to enable all origins
            // this is added just in case the OH participants
            // needed to test locally.
            services.AddCors(options =>
            {
                options.AddPolicy(_CORSPolicyName,
                    builder =>
                       {
                           builder.AllowAnyOrigin()
                               .AllowAnyMethod()
                               .AllowAnyHeader();
                       });
            });

            services
                .AddAuthentication(AzureADB2CDefaults.BearerAuthenticationScheme)
                .AddAzureADB2CBearer(options => Configuration.Bind("AzureAdB2C", options));

            services.AddMvc().AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.IgnoreNullValues = true;
                });

            // Enable Application Insights
            services.AddApplicationInsightsTelemetry(Configuration.GetSection("ApplicationInsights")["InstrumentationKey"]);

            // Swagger Document Generation
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "LinkyLink API",
                    Description = "OpenHack - Production ASP.NET Core Web API",
                    License = new OpenApiLicense
                    {
                        Name = "Use under MIT License",
                        Url = new Uri("https://raw.githubusercontent.com/Azure-Samples/openhack-production/master/LICENSE"),
                    }
                });
                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetService<LinksContext>();
                dbContext.Database.EnsureCreated();
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Configure Swagger
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });

            app.UseCors(_CORSPolicyName);

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
