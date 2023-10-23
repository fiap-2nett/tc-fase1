using Xunit;
using System;
using System.Net;
using System.Linq;
using System.Net.Http;
using FluentAssertions;
using System.Threading.Tasks;
using TechChallenge.Api.Contracts;
using TechChallenge.Domain.Errors;
using Microsoft.AspNetCore.TestHost;
using TechChallenge.Api.Controllers;
using TechChallenge.Domain.Enumerations;
using TechChallenge.Application.Contracts.Common;
using TechChallenge.Api.IntegrationTests.SeedWork;
using TechChallenge.Api.IntegrationTests.Fixtures;
using TechChallenge.Application.Contracts.Tickets;
using TechChallenge.Api.IntegrationTests.Extensions;

namespace TechChallenge.Api.IntegrationTests.Scenarios
{
    [Collection(nameof(ApiCollection))]
    public sealed class TicketsControllerIntegrationTests : IAsyncLifetime
    {
        #region Read-Only Fields

        private readonly TestHostFixture _testHostFixture;
        private readonly Func<Task> _resetDatabase;

        private readonly TicketFixture _ticketFixture;
        private readonly CategoryFixture _categoryFixture;
        private readonly TicketStatusFixture _ticketStatusFixture;

        #endregion

        #region Constructors

        public TicketsControllerIntegrationTests(TestHostFixture fixture)
        {
            _testHostFixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
            _resetDatabase = fixture.ResetDatabaseAsync;

            _ticketFixture = new TicketFixture(_testHostFixture);
            _categoryFixture = new CategoryFixture(_testHostFixture);
            _ticketStatusFixture = new TicketStatusFixture(_testHostFixture);
        }

        #endregion

        #region Tests

        #region Get

        [Fact]
        public async Task Get_ReturnsPagedListTicketResponse_WhenAuthenticatedWithUser()
        {
            // Arrange
            var ticketFixtureResult = await _ticketFixture.SetFixtureAsync();

            var parameters = new { Page = 1, PageSize = 10 };
            var userRequester = ticketFixtureResult.Users.FirstOrDefault(x => x.IdRole == (byte)UserRoles.General);
            var userRequesterTickets = ticketFixtureResult.Tickets.Where(x => x.IdUserRequester == userRequester.Id);

            // Act
            var response = await _testHostFixture.Server
                .CreateHttpApiRequest<TicketsController>(controller => controller.Get(parameters.Page, parameters.PageSize))
                .WithIdentity(userRequester)
                .GetAsync();

            // Assert
            await response.IsSuccessStatusCodeOrThrow();
            var responseContent = await response.ReadJsonContentAsAsync<PagedList<TicketResponse>>();

            responseContent.Should().NotBeNull();
            responseContent.Page.Should().Be(parameters.Page);
            responseContent.PageSize.Should().Be(parameters.PageSize);
            responseContent.TotalCount.Should().Be(userRequesterTickets.Count());

            responseContent.Items.Should().NotBeNullOrEmpty();
            responseContent.Items.Should().HaveCountLessThanOrEqualTo(parameters.PageSize);
            responseContent.Items.All(x => x.IdUserRequester == userRequester.Id).Should().BeTrue();
        }

        #endregion

        #region GetById

        [Fact]
        public async Task GetById_ReturnsUnathorized_WhenAnonymousUser()
        {
            // Arrange
            var parameters = new { Page = 1, PageSize = 10 };

            // Act
            var response = await _testHostFixture.Server
                .CreateHttpApiRequest<TicketsController>(controller => controller.Get(parameters.Page, parameters.PageSize))
                .GetAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetById_ReturnsDetailedTicketResponse_WhenAuthenticatedWithUser()
        {
            // Arrange
            var ticketFixtureResult = await _ticketFixture.SetFixtureAsync();

            var targetTicket = ticketFixtureResult.Tickets.FirstOrDefault(x => x.IdStatus == (byte)TicketStatuses.Completed);
            var ticketUserRequester = ticketFixtureResult.Users.FirstOrDefault(x => x.Id == targetTicket.IdUserRequester);
            var ticketStatus = (await _ticketStatusFixture.SetFixtureAsync()).FirstOrDefault(x => x.Id == targetTicket.IdStatus);
            var ticketCategory = (await _categoryFixture.SetFixtureAsync()).Categories.FirstOrDefault(x => x.Id == targetTicket.IdCategory);

            // Act
            var response = await _testHostFixture.Server
                .CreateHttpApiRequest<TicketsController>(controller => controller.GetById(targetTicket.Id))
                .WithIdentity(ticketUserRequester)
                .GetAsync();

            // Assert
            await response.IsSuccessStatusCodeOrThrow();
            var responseContent = await response.ReadJsonContentAsAsync<DetailedTicketResponse>();

            responseContent.Should().NotBeNull();
            responseContent.IdTicket.Should().Be(targetTicket.Id);
            responseContent.Description.Should().Be(targetTicket.Description);
            responseContent.CreatedAt.Should().Be(targetTicket.CreatedAt);
            responseContent.LastUpdatedAt.Should().Be(targetTicket.LastUpdatedAt);
            responseContent.LastUpdatedBy.Should().Be(targetTicket.LastUpdatedBy);
            responseContent.CancellationReason.Should().Be(targetTicket.CancellationReason);
            responseContent.IdUserRequester.Should().Be(targetTicket.IdUserRequester);
            responseContent.IdUserAssigned.Should().Be(targetTicket.IdUserAssigned);

            responseContent.Category.Should().NotBeNull();
            responseContent.Category.IdCategory.Should().Be(ticketCategory.Id);
            responseContent.Category.Name.Should().Be(ticketCategory.Name);

            responseContent.Status.Should().NotBeNull();
            responseContent.Status.IdStatus.Should().Be(ticketStatus.Id);
            responseContent.Status.Name.Should().Be(ticketStatus.Name);
        }

        [Fact]
        public async Task GetById_ReturnsUnauthorized_WhenAnonymousUser()
        {
            // Act
            var response = await _testHostFixture.Server
                .CreateHttpApiRequest<TicketsController>(controller => controller.GetById(int.MaxValue))
                .GetAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenInvalidTicket()
        {
            // Arrange
            var ticketFixtureResult = await _ticketFixture.SetFixtureAsync();

            var userRequester = ticketFixtureResult.Users.FirstOrDefault(x => x.IdRole == (byte)UserRoles.General);

            // Act
            var response = await _testHostFixture.Server
                .CreateHttpApiRequest<TicketsController>(controller => controller.GetById(int.MaxValue))
                .WithIdentity(userRequester)
                .GetAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            var responseContent = await response.ReadContentAsAsync<ApiErrorResponse>();

            responseContent.Should().NotBeNull();
            responseContent.Errors.Should().Contain(DomainErrors.Ticket.NotFound);
        }

        [Fact]
        public async Task GetById_ReturnsForbidden_WhenGetTicketFromAnotherUsersWithGeneralUser()
        {
            // Arrange
            var ticketFixtureResult = await _ticketFixture.SetFixtureAsync();

            var userRequester = ticketFixtureResult.Users.FirstOrDefault(x => x.IdRole == (byte)UserRoles.General);
            var targetTicket = ticketFixtureResult.Tickets.FirstOrDefault(x => x.IdUserRequester != userRequester.Id);

            // Act
            var response = await _testHostFixture.Server
                .CreateHttpApiRequest<TicketsController>(controller => controller.GetById(targetTicket.Id))
                .WithIdentity(userRequester)
                .GetAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            var responseContent = await response.ReadContentAsAsync<ApiErrorResponse>();

            responseContent.Should().NotBeNull();
            responseContent.Errors.Should().Contain(DomainErrors.User.InvalidPermissions);
        }

        [Fact]
        public async Task GetById_ReturnsForbidden_WhenGetTicketFromAnotherUsersWithAnalystUser()
        {
            // Arrange
            var ticketFixtureResult = await _ticketFixture.SetFixtureAsync();

            var userRequester = ticketFixtureResult.Users.FirstOrDefault(x => x.IdRole == (byte)UserRoles.Analyst);
            var targetTicket = ticketFixtureResult.Tickets.FirstOrDefault(x => x.IdUserAssigned != userRequester.Id);

            // Act
            var response = await _testHostFixture.Server
                .CreateHttpApiRequest<TicketsController>(controller => controller.GetById(targetTicket.Id))
                .WithIdentity(userRequester)
                .GetAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            var responseContent = await response.ReadContentAsAsync<ApiErrorResponse>();

            responseContent.Should().NotBeNull();
            responseContent.Errors.Should().Contain(DomainErrors.User.InvalidPermissions);
        }

        #endregion

        #region Create

        [Fact]
        public async Task Create_ReturnsOk_WhenValidParameters()
        {
            // Arrange
            var ticketFixtureResult = await _ticketFixture.SetFixtureAsync();

            var category = (await _categoryFixture.SetFixtureAsync()).Categories.FirstOrDefault();
            var userRequester = ticketFixtureResult.Users.FirstOrDefault(x => x.IdRole == (byte)UserRoles.General);
            var createTicketRequest = new CreateTicketRequest { IdCategory = category.Id, Description = "Lorem ipsum dolor sit amet." };

            // Act
            var response = await _testHostFixture.Server
                .CreateHttpApiRequest<TicketsController>(controller => controller.Create(createTicketRequest))
                .WithIdentity(userRequester)
                .PostAsync();

            // Assert
            await response.IsSuccessStatusCodeOrThrow();
            var responseContent = await response.ReadContentAsAsync<int?>();

            responseContent.Should().NotBeNull();
            response.Headers.Location.Should().NotBeNull();
            response.Headers.Location.AbsoluteUri.Should().Be($"{response.RequestMessage.RequestUri.AbsoluteUri}/{responseContent}");
        }

        [Fact]
        public async Task Create_ReturnsUnauthorized_WhenAnonymousUsers()
        {
            // Arrange
            var ticketFixtureResult = await _ticketFixture.SetFixtureAsync();

            var category = (await _categoryFixture.SetFixtureAsync()).Categories.FirstOrDefault();
            var userRequester = ticketFixtureResult.Users.FirstOrDefault(x => x.IdRole == (byte)UserRoles.General);
            var createTicketRequest = new CreateTicketRequest { IdCategory = category.Id, Description = "Lorem ipsum dolor sit amet." };

            // Act
            var response = await _testHostFixture.Server
                .CreateHttpApiRequest<TicketsController>(controller => controller.Create(createTicketRequest))
                .PostAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Create_ReturnsBadRequest_WhenNullRequest()
        {
            // Arrange
            var ticketFixtureResult = await _ticketFixture.SetFixtureAsync();
            var userRequester = ticketFixtureResult.Users.FirstOrDefault(x => x.IdRole == (byte)UserRoles.General);

            // Act
            var response = await _testHostFixture.Server
                .CreateHttpApiRequest<TicketsController>(controller => controller.Create((CreateTicketRequest)null))
                .WithIdentity(userRequester)
                .PostAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var responseContent = await response.ReadContentAsAsync<ApiErrorResponse>();

            responseContent.Should().NotBeNull();
            responseContent.Errors.Should().Contain(DomainErrors.Ticket.DataSentIsInvalid);
        }

        [Fact]
        public async Task Create_ReturnsBadRequest_WhenInvalidCategory()
        {
            // Arrange
            var ticketFixtureResult = await _ticketFixture.SetFixtureAsync();

            var userRequester = ticketFixtureResult.Users.FirstOrDefault(x => x.IdRole == (byte)UserRoles.General);
            var createTicketRequest = new CreateTicketRequest { IdCategory = default, Description = "Lorem ipsum dolor sit amet." };

            // Act
            var response = await _testHostFixture.Server
                .CreateHttpApiRequest<TicketsController>(controller => controller.Create(createTicketRequest))
                .WithIdentity(userRequester)
                .PostAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            var responseContent = await response.ReadContentAsAsync<ApiErrorResponse>();

            responseContent.Should().NotBeNull();
            responseContent.Errors.Should().Contain(DomainErrors.Category.NotFound);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task Create_ReturnsBadRequest_WhenInvalidDescription(string description)
        {
            // Arrange
            var ticketFixtureResult = await _ticketFixture.SetFixtureAsync();

            var category = (await _categoryFixture.SetFixtureAsync()).Categories.FirstOrDefault();
            var userRequester = ticketFixtureResult.Users.FirstOrDefault(x => x.IdRole == (byte)UserRoles.General);
            var createTicketRequest = new CreateTicketRequest { IdCategory = category.Id, Description = description };

            // Act
            var response = await _testHostFixture.Server
                .CreateHttpApiRequest<TicketsController>(controller => controller.Create(createTicketRequest))
                .WithIdentity(userRequester)
                .PostAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var responseContent = await response.ReadContentAsAsync<ApiErrorResponse>();

            responseContent.Should().NotBeNull();
            responseContent.Errors.Should().Contain(DomainErrors.Ticket.DescriptionIsRequired);
        }

        #endregion

        #region AssignToMe

        [Fact]
        public async Task AssignToMe_ReturnsOk_WhenValidParameters()
        {
            // Arrange
            var ticketFixtureResult = await _ticketFixture.SetFixtureAsync();

            var analystUser = ticketFixtureResult.Users.FirstOrDefault(x => x.IdRole == (byte)UserRoles.Analyst);
            var targetTicket = ticketFixtureResult.Tickets.FirstOrDefault(x => x.IdStatus == (byte)TicketStatuses.New);

            // Act
            var response = await _testHostFixture.Server
                .CreateHttpApiRequest<TicketsController>(controller => controller.AssignToMe(targetTicket.Id))
                .WithIdentity(analystUser)
                .PostAsync();

            // Assert
            await response.IsSuccessStatusCodeOrThrow();
        }

        [Fact]
        public async Task AssignToMe_ReturnsUnauthorized_WhenAnonymousUser()
        {
            // Arrange
            var ticketFixtureResult = await _ticketFixture.SetFixtureAsync();
            var targetTicket = ticketFixtureResult.Tickets.FirstOrDefault(x => x.IdStatus == (byte)TicketStatuses.New);

            // Act
            var response = await _testHostFixture.Server
                .CreateHttpApiRequest<TicketsController>(controller => controller.AssignToMe(targetTicket.Id))
                .PostAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task AssignToMe_ReturnsNotFound_WhenInvalidTicket()
        {
            // Arrange
            var ticketFixtureResult = await _ticketFixture.SetFixtureAsync();
            var analystUser = ticketFixtureResult.Users.FirstOrDefault(x => x.IdRole == (byte)UserRoles.Analyst);

            // Act
            var response = await _testHostFixture.Server
                .CreateHttpApiRequest<TicketsController>(controller => controller.AssignToMe(default))
                .WithIdentity(analystUser)
                .PostAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            var responseContent = await response.ReadContentAsAsync<ApiErrorResponse>();

            responseContent.Should().NotBeNull();
            responseContent.Errors.Should().Contain(DomainErrors.Ticket.NotFound);
        }

        [Fact]
        public async Task AssignToMe_ReturnsForbidden_WhenInvalidUserAssignedRole()
        {
            // Arrange
            var ticketFixtureResult = await _ticketFixture.SetFixtureAsync();

            var targetTicket = ticketFixtureResult.Tickets.FirstOrDefault(x => x.IdStatus == (byte)TicketStatuses.New);
            var assignedUser = ticketFixtureResult.Users.FirstOrDefault(x => x.IdRole == (byte)UserRoles.General && x.Id != targetTicket.IdUserRequester);

            // Act
            var response = await _testHostFixture.Server
                .CreateHttpApiRequest<TicketsController>(controller => controller.AssignToMe(targetTicket.Id))
                .WithIdentity(assignedUser)
                .PostAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            var responseContent = await response.ReadContentAsAsync<ApiErrorResponse>();

            responseContent.Should().NotBeNull();
            responseContent.Errors.Should().Contain(DomainErrors.Ticket.CannotBeAssignedToThisUser);
        }

        [Theory]
        [InlineData(TicketStatuses.Completed)]
        [InlineData(TicketStatuses.Cancelled)]
        public async Task AssignToMe_ReturnsForbidden_WhenCompletedOrCanceledTickets(TicketStatuses ticketStatuses)
        {
            // Arrange
            var ticketFixtureResult = await _ticketFixture.SetFixtureAsync();

            var targetTicket = ticketFixtureResult.Tickets.FirstOrDefault(x => x.IdStatus == (byte)ticketStatuses);
            var assignedUser = ticketFixtureResult.Users.FirstOrDefault(x => x.IdRole == (byte)UserRoles.Analyst && x.Id != targetTicket.IdUserAssigned);

            // Act
            var response = await _testHostFixture.Server
                .CreateHttpApiRequest<TicketsController>(controller => controller.AssignToMe(targetTicket.Id))
                .WithIdentity(assignedUser)
                .PostAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            var responseContent = await response.ReadContentAsAsync<ApiErrorResponse>();

            responseContent.Should().NotBeNull();
            responseContent.Errors.Should().Contain(DomainErrors.Ticket.HasAlreadyBeenCompletedOrCancelled);
        }

        #endregion

        #region Update

        [Fact]
        public async Task Update_ReturnsOk_WhenValidParameters()
        {
            // Arrange
            var ticketFixtureResult = await _ticketFixture.SetFixtureAsync();

            var targetTicket = ticketFixtureResult.Tickets.FirstOrDefault(x => x.IdStatus == (byte)TicketStatuses.New);
            var userPerformedAction = ticketFixtureResult.Users.FirstOrDefault(x => x.Id == targetTicket.IdUserRequester);
            var category = (await _categoryFixture.SetFixtureAsync()).Categories.FirstOrDefault(x => x.Id != targetTicket.IdCategory);

            var updateTicketRequest = new UpdateTicketRequest { Description = "Lorem ipsum dolor sit amet.", IdCategory = category.Id };

            // Act
            var response = await _testHostFixture.Server
                .CreateHttpApiRequest<TicketsController>(controller => controller.Update(targetTicket.Id, updateTicketRequest))
                .WithIdentity(userPerformedAction)
                .PutAsync();

            // Assert
            await response.IsSuccessStatusCodeOrThrow();
        }

        [Fact]
        public async Task Update_ReturnsUnauthorized_WhenAnonymousUser()
        {
            // Arrange
            var ticketFixtureResult = await _ticketFixture.SetFixtureAsync();

            var targetTicket = ticketFixtureResult.Tickets.FirstOrDefault(x => x.IdStatus == (byte)TicketStatuses.New);
            var userPerformedAction = ticketFixtureResult.Users.FirstOrDefault(x => x.Id == targetTicket.IdUserRequester);
            var category = (await _categoryFixture.SetFixtureAsync()).Categories.FirstOrDefault(x => x.Id != targetTicket.IdCategory);

            var updateTicketRequest = new UpdateTicketRequest { Description = "Lorem ipsum dolor sit amet.", IdCategory = category.Id };

            // Act
            var response = await _testHostFixture.Server
                .CreateHttpApiRequest<TicketsController>(controller => controller.Update(targetTicket.Id, updateTicketRequest))
                .PutAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Update_ReturnsBadRequest_WhenNullRequest()
        {
            // Arrange
            var ticketFixtureResult = await _ticketFixture.SetFixtureAsync();

            var targetTicket = ticketFixtureResult.Tickets.FirstOrDefault(x => x.IdStatus == (byte)TicketStatuses.New);
            var userPerformedAction = ticketFixtureResult.Users.FirstOrDefault(x => x.Id == targetTicket.IdUserRequester);
            var category = (await _categoryFixture.SetFixtureAsync()).Categories.FirstOrDefault(x => x.Id != targetTicket.IdCategory);

            // Act
            var response = await _testHostFixture.Server
                .CreateHttpApiRequest<TicketsController>(controller => controller.Update(targetTicket.Id, (UpdateTicketRequest)null))
                .WithIdentity(userPerformedAction)
                .PutAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var responseContent = await response.ReadContentAsAsync<ApiErrorResponse>();

            responseContent.Should().NotBeNull();
            responseContent.Errors.Should().Contain(DomainErrors.Ticket.DataSentIsInvalid);
        }

        [Fact]
        public async Task Update_ReturnsBadRequest_WhenInvalidCategory()
        {
            // Arrange
            var ticketFixtureResult = await _ticketFixture.SetFixtureAsync();

            var targetTicket = ticketFixtureResult.Tickets.FirstOrDefault(x => x.IdStatus == (byte)TicketStatuses.New);
            var userPerformedAction = ticketFixtureResult.Users.FirstOrDefault(x => x.Id == targetTicket.IdUserRequester);

            var updateTicketRequest = new UpdateTicketRequest { Description = "Lorem ipsum dolor sit amet.", IdCategory = default };

            // Act
            var response = await _testHostFixture.Server
                .CreateHttpApiRequest<TicketsController>(controller => controller.Update(targetTicket.Id, updateTicketRequest))
                .WithIdentity(userPerformedAction)
                .PutAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            var responseContent = await response.ReadContentAsAsync<ApiErrorResponse>();

            responseContent.Should().NotBeNull();
            responseContent.Errors.Should().Contain(DomainErrors.Category.NotFound);
        }

        [Fact]
        public async Task Update_ReturnsNotFound_WhenInvalidTicket()
        {
            // Arrange
            var ticketFixtureResult = await _ticketFixture.SetFixtureAsync();

            var targetTicket = ticketFixtureResult.Tickets.FirstOrDefault(x => x.IdStatus == (byte)TicketStatuses.New);
            var userPerformedAction = ticketFixtureResult.Users.FirstOrDefault(x => x.Id == targetTicket.IdUserRequester);
            var category = (await _categoryFixture.SetFixtureAsync()).Categories.FirstOrDefault(x => x.Id != targetTicket.IdCategory);

            var updateTicketRequest = new UpdateTicketRequest { Description = "Lorem ipsum dolor sit amet.", IdCategory = category.Id };

            // Act
            var response = await _testHostFixture.Server
                .CreateHttpApiRequest<TicketsController>(controller => controller.Update(default, updateTicketRequest))
                .WithIdentity(userPerformedAction)
                .PutAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            var responseContent = await response.ReadContentAsAsync<ApiErrorResponse>();

            responseContent.Should().NotBeNull();
            responseContent.Errors.Should().Contain(DomainErrors.Ticket.NotFound);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task Update_ReturnsBadRequest_WhenInvalidDescription(string description)
        {
            // Arrange
            var ticketFixtureResult = await _ticketFixture.SetFixtureAsync();

            var targetTicket = ticketFixtureResult.Tickets.FirstOrDefault(x => x.IdStatus == (byte)TicketStatuses.New);
            var userPerformedAction = ticketFixtureResult.Users.FirstOrDefault(x => x.Id == targetTicket.IdUserRequester);
            var category = (await _categoryFixture.SetFixtureAsync()).Categories.FirstOrDefault(x => x.Id != targetTicket.IdCategory);

            var updateTicketRequest = new UpdateTicketRequest { Description = description, IdCategory = category.Id };

            // Act
            var response = await _testHostFixture.Server
                .CreateHttpApiRequest<TicketsController>(controller => controller.Update(targetTicket.Id, updateTicketRequest))
                .WithIdentity(userPerformedAction)
                .PutAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var responseContent = await response.ReadContentAsAsync<ApiErrorResponse>();

            responseContent.Should().NotBeNull();
            responseContent.Errors.Should().Contain(DomainErrors.Ticket.DescriptionIsRequired);
        }

        [Fact]
        public async Task Update_ReturnsForbidden_WhenInvalidUserRequester()
        {
            // Arrange
            var ticketFixtureResult = await _ticketFixture.SetFixtureAsync();

            var targetTicket = ticketFixtureResult.Tickets.FirstOrDefault(x => x.IdStatus == (byte)TicketStatuses.New);
            var userPerformedAction = ticketFixtureResult.Users.FirstOrDefault(x => x.Id != targetTicket.IdUserRequester);
            var category = (await _categoryFixture.SetFixtureAsync()).Categories.FirstOrDefault(x => x.Id != targetTicket.IdCategory);

            var updateTicketRequest = new UpdateTicketRequest { Description = "Lorem ipsum dolor sit amet.", IdCategory = category.Id };

            // Act
            var response = await _testHostFixture.Server
                .CreateHttpApiRequest<TicketsController>(controller => controller.Update(targetTicket.Id, updateTicketRequest))
                .WithIdentity(userPerformedAction)
                .PutAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            var responseContent = await response.ReadContentAsAsync<ApiErrorResponse>();

            responseContent.Should().NotBeNull();
            responseContent.Errors.Should().Contain(DomainErrors.User.InvalidPermissions);
        }

        [Theory]
        [InlineData(TicketStatuses.Completed)]
        [InlineData(TicketStatuses.Cancelled)]
        public async Task Update_ReturnsForbidden_WhenCompletedOrCancelledTicket(TicketStatuses ticketStatus)
        {
            // Arrange
            var ticketFixtureResult = await _ticketFixture.SetFixtureAsync();

            var targetTicket = ticketFixtureResult.Tickets.FirstOrDefault(x => x.IdStatus == (byte)ticketStatus);
            var userPerformedAction = ticketFixtureResult.Users.FirstOrDefault(x => x.Id == targetTicket.IdUserRequester);
            var category = (await _categoryFixture.SetFixtureAsync()).Categories.FirstOrDefault(x => x.Id != targetTicket.IdCategory);

            var updateTicketRequest = new UpdateTicketRequest { Description = "Lorem ipsum dolor sit amet.", IdCategory = category.Id };

            // Act
            var response = await _testHostFixture.Server
                .CreateHttpApiRequest<TicketsController>(controller => controller.Update(targetTicket.Id, updateTicketRequest))
                .WithIdentity(userPerformedAction)
                .PutAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            var responseContent = await response.ReadContentAsAsync<ApiErrorResponse>();

            responseContent.Should().NotBeNull();
            responseContent.Errors.Should().Contain(DomainErrors.Ticket.HasAlreadyBeenCompletedOrCancelled);
        }

        #endregion

        #region Cancel

        [Fact]
        public async Task Cancel_ReturnsOk_WhenValidParameters()
        {
            // Arrange
            var ticketFixtureResult = await _ticketFixture.SetFixtureAsync();

            var targetTicket = ticketFixtureResult.Tickets.FirstOrDefault(x => x.IdStatus == (byte)TicketStatuses.New);
            var userPerformedAction = ticketFixtureResult.Users.FirstOrDefault(x => x.Id == targetTicket.IdUserRequester);

            var cancelTicketRequest = new CancelTicketRequest { CancellationReason = "Lorem ipsum dolor sit amet." };

            // Act
            var response = await _testHostFixture.Server
                .CreateHttpApiRequest<TicketsController>(controller => controller.Cancel(targetTicket.Id, cancelTicketRequest))
                .WithIdentity(userPerformedAction)
                .PostAsync();

            // Assert
            await response.IsSuccessStatusCodeOrThrow();
        }

        [Fact]
        public async Task Cancel_ReturnsUnauthorized_WhenAnonymousUser()
        {
            // Arrange
            var ticketFixtureResult = await _ticketFixture.SetFixtureAsync();

            var targetTicket = ticketFixtureResult.Tickets.FirstOrDefault(x => x.IdStatus == (byte)TicketStatuses.New);
            var userPerformedAction = ticketFixtureResult.Users.FirstOrDefault(x => x.Id == targetTicket.IdUserRequester);

            var cancelTicketRequest = new CancelTicketRequest { CancellationReason = "Lorem ipsum dolor sit amet." };

            // Act
            var response = await _testHostFixture.Server
                .CreateHttpApiRequest<TicketsController>(controller => controller.Cancel(targetTicket.Id, cancelTicketRequest))
                .PostAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Cancel_ReturnsBadRequest_WhenInvalidParameters()
        {
            // Arrange
            var ticketFixtureResult = await _ticketFixture.SetFixtureAsync();

            var targetTicket = ticketFixtureResult.Tickets.FirstOrDefault(x => x.IdStatus == (byte)TicketStatuses.New);
            var userPerformedAction = ticketFixtureResult.Users.FirstOrDefault(x => x.Id == targetTicket.IdUserRequester);

            // Act
            var response = await _testHostFixture.Server
                .CreateHttpApiRequest<TicketsController>(controller => controller.Cancel(targetTicket.Id, (CancelTicketRequest)null))
                .WithIdentity(userPerformedAction)
                .PostAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var responseContent = await response.ReadContentAsAsync<ApiErrorResponse>();

            responseContent.Should().NotBeNull();
            responseContent.Errors.Should().Contain(DomainErrors.Ticket.DataSentIsInvalid);
        }

        [Fact]
        public async Task Cancel_ReturnsNotFound_WhenInvalidTicket()
        {
            // Arrange
            var ticketFixtureResult = await _ticketFixture.SetFixtureAsync();

            var targetTicket = ticketFixtureResult.Tickets.FirstOrDefault(x => x.IdStatus == (byte)TicketStatuses.New);
            var userPerformedAction = ticketFixtureResult.Users.FirstOrDefault(x => x.Id == targetTicket.IdUserRequester);

            var cancelTicketRequest = new CancelTicketRequest { CancellationReason = "Lorem ipsum dolor sit amet." };

            // Act
            var response = await _testHostFixture.Server
                .CreateHttpApiRequest<TicketsController>(controller => controller.Cancel(default, cancelTicketRequest))
                .WithIdentity(userPerformedAction)
                .PostAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            var responseContent = await response.ReadContentAsAsync<ApiErrorResponse>();

            responseContent.Should().NotBeNull();
            responseContent.Errors.Should().Contain(DomainErrors.Ticket.NotFound);
        }

        [Fact]
        public async Task Cancel_ReturnsForbidden_WhenUserPerformedActionNotIsTicketUserRequester()
        {
            // Arrange
            var ticketFixtureResult = await _ticketFixture.SetFixtureAsync();

            var targetTicket = ticketFixtureResult.Tickets.FirstOrDefault(x => x.IdStatus == (byte)TicketStatuses.New);
            var userPerformedAction = ticketFixtureResult.Users.FirstOrDefault(x => x.IdRole == (byte)UserRoles.General && x.Id != targetTicket.IdUserRequester);

            var cancelTicketRequest = new CancelTicketRequest { CancellationReason = "Lorem ipsum dolor sit amet." };

            // Act
            var response = await _testHostFixture.Server
                .CreateHttpApiRequest<TicketsController>(controller => controller.Cancel(targetTicket.Id, cancelTicketRequest))
                .WithIdentity(userPerformedAction)
                .PostAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            var responseContent = await response.ReadContentAsAsync<ApiErrorResponse>();

            responseContent.Should().NotBeNull();
            responseContent.Errors.Should().Contain(DomainErrors.Ticket.CannotBeCancelledByThisUser);
        }

        [Fact]
        public async Task Cancel_ReturnsForbidden_WhenUserPerformedActionNotIsTicketUserAssigned()
        {
            // Arrange
            var ticketFixtureResult = await _ticketFixture.SetFixtureAsync();

            var targetTicket = ticketFixtureResult.Tickets.FirstOrDefault(x => x.IdStatus == (byte)TicketStatuses.Assigned);
            var userPerformedAction = ticketFixtureResult.Users.FirstOrDefault(x => x.IdRole == (byte)UserRoles.Analyst && x.Id != targetTicket.IdUserAssigned);

            var cancelTicketRequest = new CancelTicketRequest { CancellationReason = "Lorem ipsum dolor sit amet." };

            // Act
            var response = await _testHostFixture.Server
                .CreateHttpApiRequest<TicketsController>(controller => controller.Cancel(targetTicket.Id, cancelTicketRequest))
                .WithIdentity(userPerformedAction)
                .PostAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            var responseContent = await response.ReadContentAsAsync<ApiErrorResponse>();

            responseContent.Should().NotBeNull();
            responseContent.Errors.Should().Contain(DomainErrors.Ticket.CannotBeCancelledByThisUser);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task Cancel_ReturnsBadRequest_WhenInvalidCancellationReason(string cancellationReason)
        {
            // Arrange
            var ticketFixtureResult = await _ticketFixture.SetFixtureAsync();

            var targetTicket = ticketFixtureResult.Tickets.FirstOrDefault(x => x.IdStatus == (byte)TicketStatuses.New);
            var userPerformedAction = ticketFixtureResult.Users.FirstOrDefault(x => x.Id == targetTicket.IdUserRequester);

            var cancelTicketRequest = new CancelTicketRequest { CancellationReason = cancellationReason };

            // Act
            var response = await _testHostFixture.Server
                .CreateHttpApiRequest<TicketsController>(controller => controller.Cancel(targetTicket.Id, cancelTicketRequest))
                .WithIdentity(userPerformedAction)
                .PostAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var responseContent = await response.ReadContentAsAsync<ApiErrorResponse>();

            responseContent.Should().NotBeNull();
            responseContent.Errors.Should().Contain(DomainErrors.Ticket.CancellationReasonIsRequired);
        }

        [Theory]
        [InlineData(TicketStatuses.Completed)]
        [InlineData(TicketStatuses.Cancelled)]
        public async Task Cancel_ReturnsBadRequest_WhenCompletedOrCancelledTicket(TicketStatuses ticketStatus)
        {
            // Arrange
            var ticketFixtureResult = await _ticketFixture.SetFixtureAsync();

            var targetTicket = ticketFixtureResult.Tickets.FirstOrDefault(x => x.IdStatus == (byte)ticketStatus);
            var userPerformedAction = ticketFixtureResult.Users.FirstOrDefault(x => x.Id == targetTicket.IdUserRequester);

            var cancelTicketRequest = new CancelTicketRequest { CancellationReason = "Lorem ipsum dolor sit amet." };

            // Act
            var response = await _testHostFixture.Server
                .CreateHttpApiRequest<TicketsController>(controller => controller.Cancel(targetTicket.Id, cancelTicketRequest))
                .WithIdentity(userPerformedAction)
                .PostAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            var responseContent = await response.ReadContentAsAsync<ApiErrorResponse>();

            responseContent.Should().NotBeNull();
            responseContent.Errors.Should().Contain(DomainErrors.Ticket.HasAlreadyBeenCompletedOrCancelled);
        }

        #endregion

        #region AssignTo

        [Fact]
        public async Task AssignTo_ReturnsOk_WhenValidParameters()
        {
            // Arrange
            var ticketFixtureResult = await _ticketFixture.SetFixtureAsync();

            var userAssigned = ticketFixtureResult.Users.FirstOrDefault(x => x.IdRole == (byte)UserRoles.Analyst);
            var targetTicket = ticketFixtureResult.Tickets.FirstOrDefault(x => x.IdStatus == (byte)TicketStatuses.New);
            var userPerformedAction = ticketFixtureResult.Users.FirstOrDefault(x => x.Id == userAssigned.Id);

            var assignToRequest = new AssignToRequest { IdUserAssigned = userAssigned.Id };

            // Act
            var response = await _testHostFixture.Server
                .CreateHttpApiRequest<TicketsController>(controller => controller.AssignTo(targetTicket.Id, assignToRequest))
                .WithIdentity(userPerformedAction)
                .PostAsync();

            // Assert
            await response.IsSuccessStatusCodeOrThrow();
        }

        [Fact]
        public async Task AssignTo_ReturnsUnauthorized_WhenAnonymousUser()
        {
            // Arrange
            var ticketFixtureResult = await _ticketFixture.SetFixtureAsync();

            var userAssigned = ticketFixtureResult.Users.FirstOrDefault(x => x.IdRole == (byte)UserRoles.Analyst);
            var targetTicket = ticketFixtureResult.Tickets.FirstOrDefault(x => x.IdStatus == (byte)TicketStatuses.New);

            var assignToRequest = new AssignToRequest { IdUserAssigned = userAssigned.Id };

            // Act
            var response = await _testHostFixture.Server
                .CreateHttpApiRequest<TicketsController>(controller => controller.AssignTo(targetTicket.Id, assignToRequest))
                .PostAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task AssignTo_ReturnsBadRequest_WhenInvalidParameters()
        {
            // Arrange
            var ticketFixtureResult = await _ticketFixture.SetFixtureAsync();

            var userAssigned = ticketFixtureResult.Users.FirstOrDefault(x => x.IdRole == (byte)UserRoles.Analyst);
            var targetTicket = ticketFixtureResult.Tickets.FirstOrDefault(x => x.IdStatus == (byte)TicketStatuses.New);
            var userPerformedAction = ticketFixtureResult.Users.FirstOrDefault(x => x.Id == userAssigned.Id);

            // Act
            var response = await _testHostFixture.Server
                .CreateHttpApiRequest<TicketsController>(controller => controller.AssignTo(targetTicket.Id, (AssignToRequest)null))
                .WithIdentity(userPerformedAction)
                .PostAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var responseContent = await response.ReadContentAsAsync<ApiErrorResponse>();

            responseContent.Should().NotBeNull();
            responseContent.Errors.Should().Contain(DomainErrors.Ticket.DataSentIsInvalid);
        }

        [Fact]
        public async Task AssignTo_ReturnsNotFound_WhenInvalidTicket()
        {
            // Arrange
            var ticketFixtureResult = await _ticketFixture.SetFixtureAsync();

            var userAssigned = ticketFixtureResult.Users.FirstOrDefault(x => x.IdRole == (byte)UserRoles.Analyst);
            var targetTicket = ticketFixtureResult.Tickets.FirstOrDefault(x => x.IdStatus == (byte)TicketStatuses.New);
            var userPerformedAction = ticketFixtureResult.Users.FirstOrDefault(x => x.Id == userAssigned.Id);

            var assignToRequest = new AssignToRequest { IdUserAssigned = userAssigned.Id };

            // Act
            var response = await _testHostFixture.Server
                .CreateHttpApiRequest<TicketsController>(controller => controller.AssignTo(default, assignToRequest))
                .WithIdentity(userPerformedAction)
                .PostAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            var responseContent = await response.ReadContentAsAsync<ApiErrorResponse>();

            responseContent.Should().NotBeNull();
            responseContent.Errors.Should().Contain(DomainErrors.Ticket.NotFound);
        }

        [Fact]
        public async Task AssignTo_ReturnsForbidden_WhenInvalidUserAssignedRole()
        {
            // Arrange
            var ticketFixtureResult = await _ticketFixture.SetFixtureAsync();

            var targetTicket = ticketFixtureResult.Tickets.FirstOrDefault(x => x.IdStatus == (byte)TicketStatuses.New);
            var assignedUser = ticketFixtureResult.Users.FirstOrDefault(x => x.IdRole == (byte)UserRoles.General && x.Id != targetTicket.IdUserRequester);
            var userPerformedAction = ticketFixtureResult.Users.FirstOrDefault(x => x.Id == assignedUser.Id);

            var assignToRequest = new AssignToRequest { IdUserAssigned = assignedUser.Id };

            // Act
            var response = await _testHostFixture.Server
                .CreateHttpApiRequest<TicketsController>(controller => controller.AssignTo(targetTicket.Id, assignToRequest))
                .WithIdentity(assignedUser)
                .PostAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            var responseContent = await response.ReadContentAsAsync<ApiErrorResponse>();

            responseContent.Should().NotBeNull();
            responseContent.Errors.Should().Contain(DomainErrors.Ticket.CannotBeAssignedToThisUser);
        }

        [Theory]
        [InlineData(TicketStatuses.Completed)]
        [InlineData(TicketStatuses.Cancelled)]
        public async Task AssignTo_ReturnsForbidden_WhenCompletedOrCanceledTickets(TicketStatuses ticketStatuses)
        {
            // Arrange
            var ticketFixtureResult = await _ticketFixture.SetFixtureAsync();

            var userAssigned = ticketFixtureResult.Users.FirstOrDefault(x => x.IdRole == (byte)UserRoles.Analyst);
            var targetTicket = ticketFixtureResult.Tickets.FirstOrDefault(x => x.IdStatus == (byte)ticketStatuses);
            var userPerformedAction = ticketFixtureResult.Users.FirstOrDefault(x => x.Id == userAssigned.Id);

            var assignToRequest = new AssignToRequest { IdUserAssigned = userAssigned.Id };

            // Act
            var response = await _testHostFixture.Server
                .CreateHttpApiRequest<TicketsController>(controller => controller.AssignTo(targetTicket.Id, assignToRequest))
                .WithIdentity(userPerformedAction)
                .PostAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            var responseContent = await response.ReadContentAsAsync<ApiErrorResponse>();

            responseContent.Should().NotBeNull();
            responseContent.Errors.Should().Contain(DomainErrors.Ticket.HasAlreadyBeenCompletedOrCancelled);
        }

        #endregion

        #region Complete

        [Fact]
        public async Task Complete_ReturnsOk_WhenValidParameters()
        {
            // Arrange
            var ticketFixtureResult = await _ticketFixture.SetFixtureAsync();

            var targetTicket = ticketFixtureResult.Tickets.FirstOrDefault(x => x.IdStatus == (byte)TicketStatuses.Assigned);
            var userPerformedAction = ticketFixtureResult.Users.FirstOrDefault(x => x.Id == targetTicket.IdUserAssigned);

            // Act
            var response = await _testHostFixture.Server
                .CreateHttpApiRequest<TicketsController>(controller => controller.Complete(targetTicket.Id))
                .WithIdentity(userPerformedAction)
                .PostAsync();

            // Assert
            await response.IsSuccessStatusCodeOrThrow();
        }

        [Fact]
        public async Task Complete_ReturnsUnauthorized_WhenAnonymousUser()
        {
            // Arrange
            var ticketFixtureResult = await _ticketFixture.SetFixtureAsync();

            var targetTicket = ticketFixtureResult.Tickets.FirstOrDefault(x => x.IdStatus == (byte)TicketStatuses.Assigned);
            var userPerformedAction = ticketFixtureResult.Users.FirstOrDefault(x => x.Id == targetTicket.IdUserAssigned);

            // Act
            var response = await _testHostFixture.Server
                .CreateHttpApiRequest<TicketsController>(controller => controller.Complete(targetTicket.Id))
                .PostAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Complete_ReturnsNotFound_WhenInvalidTicket()
        {
            // Arrange
            var ticketFixtureResult = await _ticketFixture.SetFixtureAsync();

            var targetTicket = ticketFixtureResult.Tickets.FirstOrDefault(x => x.IdStatus == (byte)TicketStatuses.Assigned);
            var userPerformedAction = ticketFixtureResult.Users.FirstOrDefault(x => x.Id == targetTicket.IdUserAssigned);

            // Act
            var response = await _testHostFixture.Server
                .CreateHttpApiRequest<TicketsController>(controller => controller.Complete(default))
                .WithIdentity(userPerformedAction)
                .PostAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            var responseContent = await response.ReadContentAsAsync<ApiErrorResponse>();

            responseContent.Should().NotBeNull();
            responseContent.Errors.Should().Contain(DomainErrors.Ticket.NotFound);
        }

        [Fact]
        public async Task Complete_ReturnsForbidden_WhenUserPerformedActionIsGeneral()
        {
            // Arrange
            var ticketFixtureResult = await _ticketFixture.SetFixtureAsync();

            var targetTicket = ticketFixtureResult.Tickets.FirstOrDefault(x => x.IdStatus == (byte)TicketStatuses.Assigned);
            var userPerformedAction = ticketFixtureResult.Users.FirstOrDefault(x => x.Id == targetTicket.IdUserRequester);

            // Act
            var response = await _testHostFixture.Server
                .CreateHttpApiRequest<TicketsController>(controller => controller.Complete(targetTicket.Id))
                .WithIdentity(userPerformedAction)
                .PostAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            var responseContent = await response.ReadContentAsAsync<ApiErrorResponse>();

            responseContent.Should().NotBeNull();
            responseContent.Errors.Should().Contain(DomainErrors.Ticket.CannotBeCompletedByThisUser);
        }

        [Fact]
        public async Task Complete_ReturnsForbidden_WhenUserPerformedActionIsAnalystButNotUserAssigned()
        {
            // Arrange
            var ticketFixtureResult = await _ticketFixture.SetFixtureAsync();

            var targetTicket = ticketFixtureResult.Tickets.FirstOrDefault(x => x.IdStatus == (byte)TicketStatuses.Assigned);
            var userPerformedAction = ticketFixtureResult.Users.FirstOrDefault(x => x.IdRole == (byte)UserRoles.Analyst && x.Id != targetTicket.IdUserAssigned);

            // Act
            var response = await _testHostFixture.Server
                .CreateHttpApiRequest<TicketsController>(controller => controller.Complete(targetTicket.Id))
                .WithIdentity(userPerformedAction)
                .PostAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            var responseContent = await response.ReadContentAsAsync<ApiErrorResponse>();

            responseContent.Should().NotBeNull();
            responseContent.Errors.Should().Contain(DomainErrors.Ticket.CannotBeCompletedByThisUser);
        }

        [Fact]
        public async Task Complete_ReturnsBadRequest_WhenTicketStatusIsNew()
        {
            // Arrange
            var ticketFixtureResult = await _ticketFixture.SetFixtureAsync();

            var targetTicket = ticketFixtureResult.Tickets.FirstOrDefault(x => x.IdStatus == (byte)TicketStatuses.New);
            var userPerformedAction = ticketFixtureResult.Users.FirstOrDefault(x => x.IdRole == (byte)UserRoles.Administrator);

            // Act
            var response = await _testHostFixture.Server
                .CreateHttpApiRequest<TicketsController>(controller => controller.Complete(targetTicket.Id))
                .WithIdentity(userPerformedAction)
                .PostAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var responseContent = await response.ReadContentAsAsync<ApiErrorResponse>();

            responseContent.Should().NotBeNull();
            responseContent.Errors.Should().Contain(DomainErrors.Ticket.HasNotBeenAssignedToAUser);
        }

        [Theory]
        [InlineData(TicketStatuses.Completed)]
        [InlineData(TicketStatuses.Cancelled)]
        public async Task Complete_ReturnsBadRequest_WhenCompletedOrCancelledTicket(TicketStatuses ticketStatus)
        {
            // Arrange
            var ticketFixtureResult = await _ticketFixture.SetFixtureAsync();

            var targetTicket = ticketFixtureResult.Tickets.FirstOrDefault(x => x.IdStatus == (byte)ticketStatus);
            var userPerformedAction = ticketFixtureResult.Users.FirstOrDefault(x => x.IdRole == (byte)UserRoles.Administrator);

            // Act
            var response = await _testHostFixture.Server
                .CreateHttpApiRequest<TicketsController>(controller => controller.Complete(targetTicket.Id))
                .WithIdentity(userPerformedAction)
                .PostAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var responseContent = await response.ReadContentAsAsync<ApiErrorResponse>();

            responseContent.Should().NotBeNull();
            responseContent.Errors.Should().Contain(DomainErrors.Ticket.HasAlreadyBeenCompletedOrCancelled);
        }

        #endregion

        #region ChangeStatus

        [Fact]
        public async Task ChangeStatus_ReturnsOk_WhenValidParameters()
        {
            // Arrange
            var ticketFixtureResult = await _ticketFixture.SetFixtureAsync();

            var targetTicket = ticketFixtureResult.Tickets.FirstOrDefault(x => x.IdStatus == (byte)TicketStatuses.Assigned);
            var userPerformedAction = ticketFixtureResult.Users.FirstOrDefault(x => x.Id == targetTicket.IdUserAssigned);

            var changeStatusRequest = new ChangeStatusRequest { IdStatus = (byte)TicketStatuses.InProgress };

            // Act
            var response = await _testHostFixture.Server
                .CreateHttpApiRequest<TicketsController>(controller => controller.ChangeStatus(targetTicket.Id, changeStatusRequest))
                .WithIdentity(userPerformedAction)
                .PostAsync();

            // Assert
            await response.IsSuccessStatusCodeOrThrow();
        }

        [Fact]
        public async Task ChangeStatus_ReturnsUnauthorized_WhenAnonymousUser()
        {
            // Arrange
            var ticketFixtureResult = await _ticketFixture.SetFixtureAsync();

            var targetTicket = ticketFixtureResult.Tickets.FirstOrDefault(x => x.IdStatus == (byte)TicketStatuses.Assigned);
            var changeStatusRequest = new ChangeStatusRequest { IdStatus = (byte)TicketStatuses.InProgress };

            // Act
            var response = await _testHostFixture.Server
                .CreateHttpApiRequest<TicketsController>(controller => controller.ChangeStatus(targetTicket.Id, changeStatusRequest))
                .PostAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task ChangeStatus_ReturnsBadRequest_WhenInvalidParameters()
        {
            // Arrange
            var ticketFixtureResult = await _ticketFixture.SetFixtureAsync();

            var targetTicket = ticketFixtureResult.Tickets.FirstOrDefault(x => x.IdStatus == (byte)TicketStatuses.Assigned);
            var userPerformedAction = ticketFixtureResult.Users.FirstOrDefault(x => x.Id == targetTicket.IdUserAssigned);

            // Act
            var response = await _testHostFixture.Server
                .CreateHttpApiRequest<TicketsController>(controller => controller.ChangeStatus(targetTicket.Id, (ChangeStatusRequest)null))
                .WithIdentity(userPerformedAction)
                .PostAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var responseContent = await response.ReadContentAsAsync<ApiErrorResponse>();

            responseContent.Should().NotBeNull();
            responseContent.Errors.Should().Contain(DomainErrors.Ticket.DataSentIsInvalid);
        }

        [Fact]
        public async Task ChangeStatus_ReturnsBadRequest_WhenInvalidStatus()
        {
            // Arrange
            var ticketFixtureResult = await _ticketFixture.SetFixtureAsync();

            var targetTicket = ticketFixtureResult.Tickets.FirstOrDefault(x => x.IdStatus == (byte)TicketStatuses.Assigned);
            var userPerformedAction = ticketFixtureResult.Users.FirstOrDefault(x => x.Id == targetTicket.IdUserAssigned);

            var changeStatusRequest = new ChangeStatusRequest { IdStatus = default };

            // Act
            var response = await _testHostFixture.Server
                .CreateHttpApiRequest<TicketsController>(controller => controller.ChangeStatus(targetTicket.Id, changeStatusRequest))
                .WithIdentity(userPerformedAction)
                .PostAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var responseContent = await response.ReadContentAsAsync<ApiErrorResponse>();

            responseContent.Should().NotBeNull();
            responseContent.Errors.Should().Contain(DomainErrors.Ticket.StatusDoesNotExist);
        }

        [Fact]
        public async Task ChangeStatus_ReturnsNotFound_WhenInvalidTicket()
        {
            // Arrange
            var ticketFixtureResult = await _ticketFixture.SetFixtureAsync();

            var targetTicket = ticketFixtureResult.Tickets.FirstOrDefault(x => x.IdStatus == (byte)TicketStatuses.Assigned);
            var userPerformedAction = ticketFixtureResult.Users.FirstOrDefault(x => x.Id == targetTicket.IdUserAssigned);

            var changeStatusRequest = new ChangeStatusRequest { IdStatus = (byte)TicketStatuses.InProgress };

            // Act
            var response = await _testHostFixture.Server
                .CreateHttpApiRequest<TicketsController>(controller => controller.ChangeStatus(default, changeStatusRequest))
                .WithIdentity(userPerformedAction)
                .PostAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            var responseContent = await response.ReadContentAsAsync<ApiErrorResponse>();

            responseContent.Should().NotBeNull();
            responseContent.Errors.Should().Contain(DomainErrors.Ticket.NotFound);
        }

        [Fact]
        public async Task ChangeStatus_ReturnsForbidden_WhenUserPerformedActionIsGeneral()
        {
            // Arrange
            var ticketFixtureResult = await _ticketFixture.SetFixtureAsync();

            var targetTicket = ticketFixtureResult.Tickets.FirstOrDefault(x => x.IdStatus == (byte)TicketStatuses.Assigned);
            var userPerformedAction = ticketFixtureResult.Users.FirstOrDefault(x => x.IdRole == (byte)UserRoles.General);

            var changeStatusRequest = new ChangeStatusRequest { IdStatus = (byte)TicketStatuses.InProgress };

            // Act
            var response = await _testHostFixture.Server
                .CreateHttpApiRequest<TicketsController>(controller => controller.ChangeStatus(targetTicket.Id, changeStatusRequest))
                .WithIdentity(userPerformedAction)
                .PostAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            var responseContent = await response.ReadContentAsAsync<ApiErrorResponse>();

            responseContent.Should().NotBeNull();
            responseContent.Errors.Should().Contain(DomainErrors.Ticket.StatusCannotBeChangedByThisUser);
        }

        [Fact]
        public async Task ChangeStatus_ReturnsForbidden_WhenUserPerformedActionIsAnalystButNotUserAssigned()
        {
            // Arrange
            var ticketFixtureResult = await _ticketFixture.SetFixtureAsync();

            var targetTicket = ticketFixtureResult.Tickets.FirstOrDefault(x => x.IdStatus == (byte)TicketStatuses.Assigned);
            var userPerformedAction = ticketFixtureResult.Users.FirstOrDefault(x => x.IdRole == (byte)UserRoles.Analyst && x.Id != targetTicket.IdUserAssigned);

            var changeStatusRequest = new ChangeStatusRequest { IdStatus = (byte)TicketStatuses.InProgress };

            // Act
            var response = await _testHostFixture.Server
                .CreateHttpApiRequest<TicketsController>(controller => controller.ChangeStatus(targetTicket.Id, changeStatusRequest))
                .WithIdentity(userPerformedAction)
                .PostAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            var responseContent = await response.ReadContentAsAsync<ApiErrorResponse>();

            responseContent.Should().NotBeNull();
            responseContent.Errors.Should().Contain(DomainErrors.Ticket.StatusCannotBeChangedByThisUser);
        }

        [Fact]
        public async Task ChangeStatus_ReturnsBadRequest_WhenChangedStatusIsNew()
        {
            // Arrange
            var ticketFixtureResult = await _ticketFixture.SetFixtureAsync();

            var targetTicket = ticketFixtureResult.Tickets.FirstOrDefault(x => x.IdStatus == (byte)TicketStatuses.Assigned);
            var userPerformedAction = ticketFixtureResult.Users.FirstOrDefault(x => x.Id == targetTicket.IdUserAssigned);

            var changeStatusRequest = new ChangeStatusRequest { IdStatus = (byte)TicketStatuses.New };

            // Act
            var response = await _testHostFixture.Server
                .CreateHttpApiRequest<TicketsController>(controller => controller.ChangeStatus(targetTicket.Id, changeStatusRequest))
                .WithIdentity(userPerformedAction)
                .PostAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var responseContent = await response.ReadContentAsAsync<ApiErrorResponse>();

            responseContent.Should().NotBeNull();
            responseContent.Errors.Should().Contain(DomainErrors.Ticket.CannotChangeStatusToNew);
        }

        [Theory]
        [InlineData(TicketStatuses.Cancelled)]
        [InlineData(TicketStatuses.Completed)]
        public async Task ChangeStatus_ReturnsBadRequest_WhenCompletedOrCancelledTicket(TicketStatuses ticketStatus)
        {
            // Arrange
            var ticketFixtureResult = await _ticketFixture.SetFixtureAsync();

            var targetTicket = ticketFixtureResult.Tickets.FirstOrDefault(x => x.IdStatus == (byte)ticketStatus);
            var userPerformedAction = ticketFixtureResult.Users.FirstOrDefault(x => x.Id == targetTicket.IdUserAssigned);

            var changeStatusRequest = new ChangeStatusRequest { IdStatus = (byte)TicketStatuses.InProgress };

            // Act
            var response = await _testHostFixture.Server
                .CreateHttpApiRequest<TicketsController>(controller => controller.ChangeStatus(targetTicket.Id, changeStatusRequest))
                .WithIdentity(userPerformedAction)
                .PostAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var responseContent = await response.ReadContentAsAsync<ApiErrorResponse>();

            responseContent.Should().NotBeNull();
            responseContent.Errors.Should().Contain(DomainErrors.Ticket.HasAlreadyBeenCompletedOrCancelled);
        }

        [Theory]
        [InlineData(TicketStatuses.Assigned)]
        [InlineData(TicketStatuses.Cancelled)]
        public async Task ChangeStatus_ReturnsBadRequest_WhenNotAllowedChangedStatus(TicketStatuses changedStatus)
        {
            // Arrange
            var ticketFixtureResult = await _ticketFixture.SetFixtureAsync();

            var targetTicket = ticketFixtureResult.Tickets.FirstOrDefault(x => x.IdStatus == (byte)TicketStatuses.Assigned);
            var userPerformedAction = ticketFixtureResult.Users.FirstOrDefault(x => x.Id == targetTicket.IdUserAssigned);

            var changeStatusRequest = new ChangeStatusRequest { IdStatus = (byte)changedStatus };

            // Act
            var response = await _testHostFixture.Server
                .CreateHttpApiRequest<TicketsController>(controller => controller.ChangeStatus(targetTicket.Id, changeStatusRequest))
                .WithIdentity(userPerformedAction)
                .PostAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var responseContent = await response.ReadContentAsAsync<ApiErrorResponse>();

            responseContent.Should().NotBeNull();
            responseContent.Errors.Should().Contain(DomainErrors.Ticket.StatusNotAllowed);
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
