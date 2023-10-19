using Xunit;
using System;
using System.Net;
using System.Net.Http;
using FluentAssertions;
using System.Threading.Tasks;
using TechChallenge.Api.Contracts;
using TechChallenge.Domain.Errors;
using Microsoft.AspNetCore.TestHost;
using TechChallenge.Api.Controllers;
using TechChallenge.Domain.Enumerations;
using TechChallenge.Application.Contracts.Users;
using TechChallenge.Application.Contracts.Common;
using TechChallenge.Api.IntegrationTests.SeedWork;
using TechChallenge.Api.IntegrationTests.Fixtures;
using TechChallenge.Api.IntegrationTests.Extensions;
using System.Linq;

namespace TechChallenge.Api.IntegrationTests.Scenarios
{
    [Collection(nameof(ApiCollection))]
    public sealed class UsersControllerIntegrationTests : IAsyncLifetime
    {
        #region Read-Only Fields

        private readonly TestHostFixture _testHostFixture;

        private readonly UserFixture _userFixture;
        private readonly Func<Task> _resetDatabase;

        #endregion

        #region Constructors

        public UsersControllerIntegrationTests(TestHostFixture fixture)
        {
            _testHostFixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
            _userFixture = new UserFixture(_testHostFixture);
            _resetDatabase = fixture.ResetDatabaseAsync;
        }

        #endregion

        #region Tests

        #region Get

        [Fact]
        public async Task Get_ReturnsPagedListUserResponse_WhenValidParameters()
        {
            // Arrange
            var fixtureResult = await _userFixture.SetFixtureAsync();
            var userPerformedAction = fixtureResult.FirstOrDefault();

            var parameters = new { Page = 1, PageSize = 10, Items = fixtureResult.Count() };

            // Act
            var response = await _testHostFixture.Server
                .CreateHttpApiRequest<UsersController>(controller => controller.Get(parameters.Page, parameters.PageSize))
                .WithIdentity(userPerformedAction)
                .GetAsync();

            // Assert
            await response.IsSuccessStatusCodeOrThrow();
            var responseContent = await response.ReadJsonContentAsAsync<PagedList<UserResponse>>();

            responseContent.Should().NotBeNull();
            responseContent.Page.Should().Be(parameters.Page);
            responseContent.PageSize.Should().Be(parameters.PageSize);
            responseContent.TotalCount.Should().BeGreaterThanOrEqualTo(parameters.Items);

            responseContent.Items.Should().NotBeNullOrEmpty();
            responseContent.Items.Should().HaveCountLessThanOrEqualTo(parameters.PageSize);
        }

        #endregion

        #region GetMyProfile

        [Fact]
        public async Task GetMyProfile_ReturnsDetailedUserResponse_WhenAuthenticatedUser()
        {
            // Arrange
            var userPerformedAction = (await _userFixture.SetFixtureAsync()).FirstOrDefault();

            // Act
            var response = await _testHostFixture.Server
                .CreateHttpApiRequest<UsersController>(controller => controller.GetMyProfile())
                .WithIdentity(userPerformedAction)
                .GetAsync();

            // Assert
            await response.IsSuccessStatusCodeOrThrow();
            var responseContent = await response.ReadJsonContentAsAsync<DetailedUserResponse>();

            responseContent.Should().NotBeNull();
            responseContent.IdUser.Should().Be(userPerformedAction.Id);
            responseContent.Name.Should().Be(userPerformedAction.Name);
            responseContent.Surname.Should().Be(userPerformedAction.Surname);
            responseContent.Email.Should().Be(userPerformedAction.Email);

            responseContent.Role.Should().NotBeNull();
            responseContent.Role.IdRole.Should().Be(userPerformedAction.IdRole);

            responseContent.CreatedAt.Should().Be(userPerformedAction.CreatedAt);
            responseContent.LastUpdatedAt.Should().Be(userPerformedAction.LastUpdatedAt);
        }

        #endregion

        #region ChangePassword

        [Fact]
        public async Task ChangePassword_ReturnsOk_WhenValidUser()
        {
            // Arrange
            var userPerformedAction = (await _userFixture.SetFixtureAsync()).FirstOrDefault();
            var changePasswordRequest = new ChangePasswordRequest { Password = "ChangePassword@123" };

            // Act
            var response = await _testHostFixture.Server
                .CreateHttpApiRequest<UsersController>(controller => controller.ChangePassword(changePasswordRequest))
                .WithIdentity(userPerformedAction)
                .PutAsync();

            // Assert
            await response.IsSuccessStatusCodeOrThrow();
        }

        #endregion

        #region Update

        [Fact]
        public async Task Update_ReturnsOk_WhenValidUser()
        {
            // Arrange
            var userPerformedAction = (await _userFixture.SetFixtureAsync()).FirstOrDefault();
            var updateUserRequest = new UpdateUserRequest { Name = "ChangedName", Surname = "ChangedSurname" };

            // Act
            var response = await _testHostFixture.Server
                .CreateHttpApiRequest<UsersController>(controller => controller.Update(updateUserRequest))
                .WithIdentity(userPerformedAction)
                .PutAsync();

            // Assert
            await response.IsSuccessStatusCodeOrThrow();
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task Update_ReturnsBadRequest_WhenInvalidName(string name)
        {
            // Arrange
            var userPerformedAction = (await _userFixture.SetFixtureAsync()).FirstOrDefault();
            var updateUserRequest = new UpdateUserRequest { Name = name, Surname = userPerformedAction.Surname };

            // Act
            var response = await _testHostFixture.Server
                .CreateHttpApiRequest<UsersController>(controller => controller.Update(updateUserRequest))
                .WithIdentity(userPerformedAction)
                .PutAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var responseContent = await response.ReadContentAsAsync<ApiErrorResponse>();

            responseContent.Should().NotBeNull();
            responseContent.Errors.Should().Contain(DomainErrors.User.NameIsRequired);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task Update_ReturnsBadRequest_WhenInvalidSurname(string surname)
        {
            // Arrange
            var userPerformedAction = (await _userFixture.SetFixtureAsync()).FirstOrDefault();
            var updateUserRequest = new UpdateUserRequest { Name = userPerformedAction.Name, Surname = surname };

            // Act
            var response = await _testHostFixture.Server
                .CreateHttpApiRequest<UsersController>(controller => controller.Update(updateUserRequest))
                .WithIdentity(userPerformedAction)
                .PutAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var responseContent = await response.ReadContentAsAsync<ApiErrorResponse>();

            responseContent.Should().NotBeNull();
            responseContent.Errors.Should().Contain(DomainErrors.User.SurnameIsRequired);
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
