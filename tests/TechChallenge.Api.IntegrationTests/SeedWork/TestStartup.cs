using System.IO;
using Acheve.TestHost;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using TechChallenge.Application;
using TechChallenge.Persistence;
using Microsoft.AspNetCore.Builder;
using TechChallenge.Api.Extensions;
using TechChallenge.Infrastructure;
using Microsoft.Extensions.Configuration;
using Acheve.AspNetCore.TestHost.Security;
using Microsoft.Extensions.DependencyInjection;

namespace TechChallenge.Api.IntegrationTests.SeedWork
{
    public sealed class TestStartup
    {
        public readonly IConfiguration _configuration;

        public TestStartup()
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddApplication()
                .AddInfrastructure(_configuration)
                .AddPersistence(_configuration);

            services
                .AddAuthentication(options => options.DefaultScheme = TestServerDefaults.AuthenticationScheme)
                .AddTestServer();

            services
                .AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.IncludeFields = true;
                    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                })
                .AddApplicationPart(AssemblyReference.Assembly);

            services.AddHttpContextAccessor();
            services.Configure<ApiBehaviorOptions>(options => options.SuppressModelStateInvalidFilter = true);
        }

        public void Configure(IApplicationBuilder app)
        {
            app.EnsureDatabaseCreated();
            app.UseCustomExceptionHandler();
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }
}
