using LinkyLink.Helpers;
using LinkyLink.Models;
using LinkyLink.Service;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LinkyLink
{
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


            services.AddMvc().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.IgnoreNullValues = true;
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

            app.UseCors(_CORSPolicyName);

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
