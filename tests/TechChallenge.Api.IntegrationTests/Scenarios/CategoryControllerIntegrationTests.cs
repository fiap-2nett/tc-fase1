using Xunit;
using System;
using System.Net;
using System.Linq;
using System.Net.Http;
using FluentAssertions;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.TestHost;
using TechChallenge.Api.Controllers;
using TechChallenge.Domain.Extensions;
using TechChallenge.Api.IntegrationTests.SeedWork;
using TechChallenge.Api.IntegrationTests.Fixtures;
using TechChallenge.Application.Contracts.Category;
using TechChallenge.Api.IntegrationTests.Extensions;

namespace TechChallenge.Api.IntegrationTests.Scenarios
{
    [Collection(nameof(ApiCollection))]
    public sealed class CategoryControllerIntegrationTests : IAsyncLifetime
    {
        #region Read-Only Fields

        private readonly TestHostFixture _testHostFixture;
        private readonly CategoryFixture _categoryFixture;
        private readonly Func<Task> _resetDatabase;
        private readonly UserFixture _userFixture;

        #endregion

        #region Constructors

        public CategoryControllerIntegrationTests(TestHostFixture fixture)
        {
            _testHostFixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
            _categoryFixture = new CategoryFixture(_testHostFixture);
            _userFixture = new UserFixture(_testHostFixture);
            _resetDatabase = fixture.ResetDatabaseAsync;
        }

        #endregion

        #region Tests

        #region Get

        [Fact]
        public async Task Get_ShouldReturnUnauthorized_WhenWithoutIdentity()
        {
            // Act
            var response = await _testHostFixture.Server
                .CreateHttpApiRequest<CategoryController>(controller => controller.Get())
                .GetAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Get_ShouldReturnDetailedCategoryResponseList_WhenAutenticatedUser()
        {
            // Arrange
            var fixtureResult = await _categoryFixture.SetFixtureAsync();
            var userPerformedAction = (await _userFixture.SetFixtureAsync()).FirstOrDefault();

            // Act
            var response = await _testHostFixture.Server
                .CreateHttpApiRequest<CategoryController>(controller => controller.Get())
                .WithIdentity(userPerformedAction)
                .GetAsync();

            // Assert
            await response.IsSuccessStatusCodeOrThrow();
            var responseContent = await response.ReadContentAsAsync<IEnumerable<CategoryResponse>>();

            responseContent.IsNullOrEmpty().Should().BeFalse();
            responseContent.Should().HaveCount(fixtureResult.Categories.Count());
            responseContent.All(x => !x.Name.IsNullOrEmpty()).Should().BeTrue();
        }

        #endregion

        #region GetById

        [Fact]
        public async Task GetById_ShouldReturnUnauthorized_WhenWithoutIdentity()
        {
            // Act
            var response = await _testHostFixture.Server
                .CreateHttpApiRequest<CategoryController>(controller => controller.Get(default))
                .GetAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetById_ShouldDetailedCategoryResponse_WhenValidId()
        {
            // Arrange
            var (categories, priorities) = await _categoryFixture.SetFixtureAsync();
            var userPerformedAction = (await _userFixture.SetFixtureAsync()).FirstOrDefault();

            var targetGategory = categories.FirstOrDefault();
            var targetPriority = priorities.FirstOrDefault(x => x.Id == targetGategory.IdPriority);

            // Act
            var response = await _testHostFixture.Server
                .CreateHttpApiRequest<CategoryController>(controller => controller.Get(targetGategory.Id))
                .WithIdentity(userPerformedAction)
                .GetAsync();

            // Assert
            await response.IsSuccessStatusCodeOrThrow();
            var responseContent = await response.ReadContentAsAsync<DetailedCategoryResponse>();

            responseContent.Should().NotBeNull();
            responseContent.IdCategory.Should().Be(targetGategory.Id);
            responseContent.Name.Should().Be(targetGategory.Name);

            responseContent.Priority.Should().NotBeNull();
            responseContent.Priority.IdPriority.Should().Be(targetPriority.Id);
            responseContent.Priority.Name.Should().Be(targetPriority.Name);
        }

        [Fact]
        public async Task GetById_ShouldReturnNotFound_WhenInvalidId()
        {
            // Arrange
            var userPerformedAction = (await _userFixture.SetFixtureAsync()).FirstOrDefault();

            // Act
            var response = await _testHostFixture.Server
                .CreateHttpApiRequest<CategoryController>(controller => controller.Get(default))
                .WithIdentity(userPerformedAction)
                .GetAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        #endregion

        #endregion

        #region IAsyncLifetime Members

        public Task InitializeAsync()
            => Task.CompletedTask;

        public Task DisposeAsync()
            => _resetDatabase();

        #endregion
    }
}
