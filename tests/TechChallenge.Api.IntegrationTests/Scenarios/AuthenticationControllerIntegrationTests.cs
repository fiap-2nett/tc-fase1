using Xunit;
using System;
using System.Net;
using System.Net.Http;
using FluentAssertions;
using System.Threading.Tasks;
using TechChallenge.Domain.Errors;
using TechChallenge.Api.Contracts;
using Microsoft.AspNetCore.TestHost;
using TechChallenge.Api.Controllers;
using TechChallenge.Domain.Enumerations;
using TechChallenge.Api.IntegrationTests.SeedWork;
using TechChallenge.Api.IntegrationTests.Fixtures;
using TechChallenge.Application.Contracts.Authentication;

namespace TechChallenge.Api.IntegrationTests.Scenarios
{
    [Collection(nameof(ApiCollection))]
    public sealed class AuthenticationControllerIntegrationTests : IAsyncLifetime
    {
        #region Read-Only Fields

        private readonly TestHostFixture _testHostFixture;

        private readonly UserFixture _userFixture;
        private readonly Func<Task> _resetDatabase;

        #endregion

        #region Constructors

        public AuthenticationControllerIntegrationTests(TestHostFixture fixture)
        {
            _testHostFixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
            _userFixture = new UserFixture(_testHostFixture);
            _resetDatabase = fixture.ResetDatabaseAsync;
        }

        #endregion

        #region Tests

        #region Login

        [Fact]
        public async Task Login_ShouldReturnTokenResponse_WhenValidCredentials()
        {
            // Arrange
            var loginRequest = new LoginRequest { Email = "john.doe@test.com", Password = "John@123" };
            await _userFixture.SetFixtureAsync(name: "John", surname: "Doe", email: loginRequest.Email, password: loginRequest.Password, role: UserRoles.General);

            // Act
            var response = await _testHostFixture.Server
                .CreateHttpApiRequest<AuthenticationController>(controller => controller.Login(loginRequest))
                .PostAsync();

            // Assert
            await response.IsSuccessStatusCodeOrThrow();
            var responseContent = await response.ReadContentAsAsync<TokenResponse>();

            responseContent.Should().NotBeNull();
            responseContent.Token.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task Login_ShouldBadRequest_WhenInvalidCredentials()
        {
            // Arrange
            var loginRequest = new LoginRequest { Email = "invalid@test.com", Password = "Invalid@123" };

            // Act
            var response = await _testHostFixture.Server
                .CreateHttpApiRequest<AuthenticationController>(controller => controller.Login(loginRequest))
                .PostAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var responseContent = await response.ReadContentAsAsync<ApiErrorResponse>();

            responseContent.Should().NotBeNull();
            responseContent.Errors.Should().Contain(DomainErrors.Authentication.InvalidEmailOrPassword);
        }

        #endregion

        #region Create

        [Fact]
        public async Task Create_ShouldReturnTokenResponse_WhenValidCredentials()
        {
            // Arrange
            var registerRequest = new RegisterRequest { Name = "John", Surname = "Doe", Email = "john.doe@test.com", Password = "John@123" };

            // Act
            var response = await _testHostFixture.Server
                .CreateHttpApiRequest<AuthenticationController>(controller => controller.Create(registerRequest))
                .PostAsync();

            // Assert
            await response.IsSuccessStatusCodeOrThrow();
            var responseContent = await response.ReadContentAsAsync<TokenResponse>();

            responseContent.Should().NotBeNull();
            responseContent.Token.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task Create_ShouldReturnBadRequest_WhenDuplicatedUser()
        {
            // Arrange
            var registerRequest = new RegisterRequest { Name = "John", Surname = "Doe", Email = "john.doe@test.com", Password = "John@123" };
            await _userFixture.SetFixtureAsync(registerRequest.Name, registerRequest.Surname, registerRequest.Email, registerRequest.Password);

            // Act
            var response = await _testHostFixture.Server
                .CreateHttpApiRequest<AuthenticationController>(controller => controller.Create(registerRequest))
                .PostAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var responseContent = await response.ReadContentAsAsync<ApiErrorResponse>();

            responseContent.Should().NotBeNull();
            responseContent.Errors.Should().Contain(DomainErrors.User.DuplicateEmail);
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
