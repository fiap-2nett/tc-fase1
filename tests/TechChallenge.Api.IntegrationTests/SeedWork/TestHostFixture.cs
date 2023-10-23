using Xunit;
using System;
using Respawn;
using Respawn.Graph;
using System.Data.Common;
using Microsoft.AspNetCore;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using TechChallenge.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using TechChallenge.Persistence.Infrastructure;
using TechChallenge.Api.IntegrationTests.Helpers;

namespace TechChallenge.Api.IntegrationTests.SeedWork
{
    public sealed class TestHostFixture : WebApplicationFactory<TestStartup>, IAsyncLifetime
    {
        #region Private Fields

        private Respawner _respawner = default!;
        private DbConnection _dbConnection = default!;

        #endregion

        #region Public Methods

        public async Task ResetDatabaseAsync()
        {
            await _respawner.ResetAsync(_dbConnection);
            await DatabaseHelper.ResetUsersTableAsync(_dbConnection);
        }

        public async Task ExecuteDbContextAsync(Func<EFContext, Task> action)
            => await ExecuteScopeAsync(sp => action(sp.GetService<EFContext>()));

        #endregion

        #region Overriden Methods

        protected override IWebHostBuilder CreateWebHostBuilder()
        {
            return WebHost.CreateDefaultBuilder();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseStartup<TestStartup>()
                .UseSolutionRelativeContentRoot("tests")
                .UseTestServer();
        }

        #endregion

        #region IAsyncLifetime Members

        public async Task InitializeAsync()
        {
            _dbConnection = new SqlConnection(Server.Host.Services.GetService<ConnectionString>());
            await InitializeRespawner();
        }

        Task IAsyncLifetime.DisposeAsync() => Task.CompletedTask;

        #endregion

        #region Private Methods

        private async Task ExecuteScopeAsync(Func<IServiceProvider, Task> action)
        {
            using var scope = Server.Host.Services.GetService<IServiceScopeFactory>().CreateScope();
            await action(scope.ServiceProvider);
        }

        private async Task InitializeRespawner()
        {
            await _dbConnection.OpenAsync();

            _respawner = await Respawner.CreateAsync(_dbConnection, new RespawnerOptions
            {
                TablesToIgnore = new Table[]
                {
                    "__EFMigrationsHistory",
                    "categories",
                    "ticketstatus",
                    "priorities",
                    "users",
                    "roles"
                },
                DbAdapter = DbAdapter.SqlServer
            });
        }

        #endregion
    }

    [CollectionDefinition(nameof(ApiCollection))]
    public class ApiCollection : ICollectionFixture<TestHostFixture>
    { }
}
