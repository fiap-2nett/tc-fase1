using Moq;
using Xunit;
using System;
using System.Linq;
using FluentAssertions;
using System.Threading.Tasks;
using Moq.EntityFrameworkCore;
using System.Collections.Generic;
using TechChallenge.Domain.Errors;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Exceptions;
using TechChallenge.Domain.Extensions;
using TechChallenge.Domain.Enumerations;
using TechChallenge.Domain.Repositories;
using TechChallenge.Domain.ValueObjects;
using TechChallenge.Application.Services;
using TechChallenge.Infrastructure.Cryptography;
using TechChallenge.Application.Contracts.Tickets;
using TechChallenge.Application.UnitTests.TestEntities;
using TechChallenge.Application.Core.Abstractions.Data;
using TechChallenge.Application.Core.Abstractions.Cryptography;

namespace TechChallenge.Application.UnitTests.Scenarios
{
    public sealed class TicketServiceTests
    {
        #region Read-Only Fields

        private readonly IPasswordHasher _passwordHasher;

        private readonly Mock<IDbContext> _dbContextMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<ITicketRepository> _ticketRepositoryMock;
        private readonly Mock<ICategoryRepository> _categoryRepositoryMock;

        #endregion

        #region Constructors

        public TicketServiceTests()
        {
            _dbContextMock = new();
            _unitOfWorkMock = new();
            _userRepositoryMock = new();
            _ticketRepositoryMock = new();
            _categoryRepositoryMock = new();

            _passwordHasher = new PasswordHasher();
        }

        #endregion

        #region Unit Tests

        [Fact]
        public async Task GetTicketByIdAsync_Should_ReturnDetailedTicketResponse_WhenValidParameters()
        {
            // Arrange
            var expectedTicket = TicketList().FirstOrDefault(x => x.IdUserRequester == UserA.Id);

            _dbContextMock.Setup(x => x.Set<Ticket, int>()).ReturnsDbSet(TicketList());
            _dbContextMock.Setup(x => x.Set<Category, int>()).ReturnsDbSet(CategoryList());
            _dbContextMock.Setup(x => x.Set<TicketStatus, byte>()).ReturnsDbSet(TicketStatusList());
            _userRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(UserA);

            var ticketService = new TicketService(_dbContextMock.Object, _unitOfWorkMock.Object, _ticketRepositoryMock.Object,
                _userRepositoryMock.Object, _categoryRepositoryMock.Object);

            // Act
            var testResult = await ticketService.GetTicketByIdAsync(expectedTicket.Id, expectedTicket.IdUserRequester);

            // Assert
            testResult.Should().NotBeNull();
            testResult.IdTicket.Should().Be(expectedTicket.Id);
            testResult.Description.Should().Be(expectedTicket.Description);
            testResult.Status.Should().NotBeNull();
            testResult.Status.IdStatus.Should().Be(expectedTicket.IdStatus);
            testResult.Status.Name.Should().Be(TicketStatusList().FirstOrDefault(x => x.Id == expectedTicket.IdStatus).Name);
            testResult.Category.Should().NotBeNull();
            testResult.Category.IdCategory.Should().Be(expectedTicket.IdCategory);
            testResult.Category.Name.Should().Be(CategoryList().FirstOrDefault(x => x.Id == expectedTicket.IdCategory).Name);
            testResult.IdUserAssigned.Should().Be(expectedTicket.IdUserAssigned);
            testResult.IdUserAssigned.Should().Be(expectedTicket.IdUserAssigned);
            testResult.CreatedAt.Date.Should().Be(expectedTicket.CreatedAt.Date);
            testResult.LastUpdatedAt.Should().Be(expectedTicket.LastUpdatedAt);
            testResult.LastUpdatedBy.Should().Be(expectedTicket.LastUpdatedBy);
            testResult.CancellationReason.Should().Be(expectedTicket.CancellationReason);

            _userRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async Task GetTicketByIdAsync_Should_ThrowNotFoundException_WhenInvalidIdUserPerformedAction()
        {
            // Arrange
            var expectedTicket = TicketList().FirstOrDefault(x => x.IdUserRequester == UserA.Id);

            _dbContextMock.Setup(x => x.Set<Ticket, int>()).ReturnsDbSet(TicketList());
            _dbContextMock.Setup(x => x.Set<Category, int>()).ReturnsDbSet(CategoryList());
            _dbContextMock.Setup(x => x.Set<TicketStatus, byte>()).ReturnsDbSet(TicketStatusList());
            _userRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((User)null);

            var ticketService = new TicketService(_dbContextMock.Object, _unitOfWorkMock.Object, _ticketRepositoryMock.Object,
                _userRepositoryMock.Object, _categoryRepositoryMock.Object);

            // Act
            var action = () => ticketService.GetTicketByIdAsync(expectedTicket.Id, expectedTicket.IdUserRequester);

            // Assert
            await action.Should()
                .ThrowAsync<NotFoundException>()
                .WithMessage(DomainErrors.User.NotFound.Message);

            _userRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async Task GetTicketByIdAsync_Should_ThrowNotFoundException_WhenInvalidIdTicket()
        {
            // Arrange
            _dbContextMock.Setup(x => x.Set<Ticket, int>()).ReturnsDbSet(TicketList());
            _dbContextMock.Setup(x => x.Set<Category, int>()).ReturnsDbSet(CategoryList());
            _dbContextMock.Setup(x => x.Set<TicketStatus, byte>()).ReturnsDbSet(TicketStatusList());

            _userRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(UserA);

            var ticketService = new TicketService(_dbContextMock.Object, _unitOfWorkMock.Object, _ticketRepositoryMock.Object,
                _userRepositoryMock.Object, _categoryRepositoryMock.Object);

            // Act
            var action = () => ticketService.GetTicketByIdAsync(int.MaxValue, UserA.Id);

            // Assert
            await action.Should()
                .ThrowAsync<NotFoundException>()
                .WithMessage(DomainErrors.Ticket.NotFound.Message);

            _userRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async Task GetTicketByIdAsync_Should_ThrowInvalidPermissionException_WhenNotSameUserRequesterFromTheTicket()
        {
            // Arrange
            var expectedTicket = TicketList().FirstOrDefault(x => x.IdUserRequester == UserA.Id);

            _dbContextMock.Setup(x => x.Set<Ticket, int>()).ReturnsDbSet(TicketList());
            _dbContextMock.Setup(x => x.Set<Category, int>()).ReturnsDbSet(CategoryList());
            _dbContextMock.Setup(x => x.Set<TicketStatus, byte>()).ReturnsDbSet(TicketStatusList());
            _userRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(UserB);

            var ticketService = new TicketService(_dbContextMock.Object, _unitOfWorkMock.Object, _ticketRepositoryMock.Object,
                _userRepositoryMock.Object, _categoryRepositoryMock.Object);

            // Act
            var action = () => ticketService.GetTicketByIdAsync(expectedTicket.Id, UserB.Id);

            // Assert
            await action.Should()
                .ThrowAsync<InvalidPermissionException>()
                .WithMessage(DomainErrors.User.InvalidPermissions.Message);

            _userRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async Task GetTicketByIdAsync_Should_ThrowInvalidPermissionException_WhenItIsNotTheUserAssignedToTheTicket()
        {
            // Arrange
            var expectedTicket = TicketList().FirstOrDefault(x => x.IdUserRequester == UserA.Id);

            _dbContextMock.Setup(x => x.Set<Ticket, int>()).ReturnsDbSet(TicketList());
            _dbContextMock.Setup(x => x.Set<Category, int>()).ReturnsDbSet(CategoryList());
            _dbContextMock.Setup(x => x.Set<TicketStatus, byte>()).ReturnsDbSet(TicketStatusList());
            _userRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(AnalystA);

            var ticketService = new TicketService(_dbContextMock.Object, _unitOfWorkMock.Object, _ticketRepositoryMock.Object,
                _userRepositoryMock.Object, _categoryRepositoryMock.Object);

            // Act
            var action = () => ticketService.GetTicketByIdAsync(expectedTicket.Id, AnalystA.Id);

            // Assert
            await action.Should()
                .ThrowAsync<InvalidPermissionException>()
                .WithMessage(DomainErrors.User.InvalidPermissions.Message);

            _userRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async Task GetTicketsAsync_Should_ReturnTicketResponsePageList_WhenValidParameters()
        {
            // Arrange
            var userPerformedAction = UserA;

            _dbContextMock.Setup(x => x.Set<Ticket, int>()).ReturnsDbSet(TicketList());
            _dbContextMock.Setup(x => x.Set<Category, int>()).ReturnsDbSet(CategoryList());
            _dbContextMock.Setup(x => x.Set<TicketStatus, byte>()).ReturnsDbSet(TicketStatusList());
            _userRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(userPerformedAction);

            var ticketService = new TicketService(_dbContextMock.Object, _unitOfWorkMock.Object, _ticketRepositoryMock.Object,
                _userRepositoryMock.Object, _categoryRepositoryMock.Object);

            // Act
            var testResult = await ticketService.GetTicketsAsync(new GetTicketsRequest(page: 1, pageSize: 10), userPerformedAction.Id);

            // Assert
            testResult.Should().NotBeNull();
            testResult.Page.Should().Be(1);
            testResult.PageSize.Should().Be(10);
            testResult.Items.IsNullOrEmpty().Should().BeFalse();
            testResult.TotalCount.Should().Be(TicketList().Count(x => x.IdUserRequester == userPerformedAction.Id));

            _userRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async Task GetTicketsAsync_Should_ThrowNotFoundException_WhenInvalidIdUserPerformedAction()
        {
            // Arrange
            var userPerformedAction = UserA;

            _dbContextMock.Setup(x => x.Set<Ticket, int>()).ReturnsDbSet(TicketList());
            _dbContextMock.Setup(x => x.Set<Category, int>()).ReturnsDbSet(CategoryList());
            _dbContextMock.Setup(x => x.Set<TicketStatus, byte>()).ReturnsDbSet(TicketStatusList());
            _userRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((User)null);

            var ticketService = new TicketService(_dbContextMock.Object, _unitOfWorkMock.Object, _ticketRepositoryMock.Object,
                _userRepositoryMock.Object, _categoryRepositoryMock.Object);

            // Act
            var action = () => ticketService.GetTicketsAsync(new GetTicketsRequest(page: 1, pageSize: 10), userPerformedAction.Id);

            // Assert
            await action.Should()
                .ThrowAsync<NotFoundException>()
                .WithMessage(DomainErrors.User.NotFound.Message);

            _userRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async Task GetTicketsAsync_Should_ReturnMyTicketResponsePageList_WhenUserPerformedActionRoleIsGeneral()
        {
            // Arrange
            var userPerformedAction = UserA;

            _dbContextMock.Setup(x => x.Set<Ticket, int>()).ReturnsDbSet(TicketList());
            _dbContextMock.Setup(x => x.Set<Category, int>()).ReturnsDbSet(CategoryList());
            _dbContextMock.Setup(x => x.Set<TicketStatus, byte>()).ReturnsDbSet(TicketStatusList());
            _userRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(userPerformedAction);

            var ticketService = new TicketService(_dbContextMock.Object, _unitOfWorkMock.Object, _ticketRepositoryMock.Object,
                _userRepositoryMock.Object, _categoryRepositoryMock.Object);

            // Act
            var testResult = await ticketService.GetTicketsAsync(new GetTicketsRequest(page: 1, pageSize: 10), userPerformedAction.Id);

            // Assert
            testResult.Should().NotBeNull();
            testResult.Page.Should().Be(1);
            testResult.PageSize.Should().Be(10);
            testResult.Items.IsNullOrEmpty().Should().BeFalse();
            testResult.Items.All(x => x.IdUserRequester == userPerformedAction.Id).Should().BeTrue();
            testResult.TotalCount.Should().Be(TicketList().Count(x => x.IdUserRequester == userPerformedAction.Id));

            _userRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async Task GetTicketsAsync_Should_ReturnAssignedOrMeTicketResponsePageList_WhenUserPerformedActionRoleIsAnalyst()
        {
            // Arrange
            var userPerformedAction = AnalystA;

            _dbContextMock.Setup(x => x.Set<Ticket, int>()).ReturnsDbSet(TicketList());
            _dbContextMock.Setup(x => x.Set<Category, int>()).ReturnsDbSet(CategoryList());
            _dbContextMock.Setup(x => x.Set<TicketStatus, byte>()).ReturnsDbSet(TicketStatusList());
            _userRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(userPerformedAction);

            var ticketService = new TicketService(_dbContextMock.Object, _unitOfWorkMock.Object, _ticketRepositoryMock.Object,
                _userRepositoryMock.Object, _categoryRepositoryMock.Object);

            // Act
            var testResult = await ticketService.GetTicketsAsync(new GetTicketsRequest(page: 1, pageSize: 10), userPerformedAction.Id);

            // Assert
            testResult.Should().NotBeNull();
            testResult.Page.Should().Be(1);
            testResult.PageSize.Should().Be(10);
            testResult.Items.IsNullOrEmpty().Should().BeFalse();
            testResult.Items.All(x => x.IdUserRequester == userPerformedAction.Id || x.IdUserAssigned == userPerformedAction.Id || x.IdUserAssigned == null).Should().BeTrue();
            testResult.TotalCount.Should().Be(TicketList().Count(x => x.IdUserRequester == userPerformedAction.Id || x.IdUserAssigned == userPerformedAction.Id || x.IdUserAssigned == null));

            _userRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async Task GetTicketsAsync_Should_ReturnAllTicketResponsePageList_WhenUserPerformedActionRoleIsAdministrator()
        {
            // Arrange
            var userPerformedAction = Admin;

            _dbContextMock.Setup(x => x.Set<Ticket, int>()).ReturnsDbSet(TicketList());
            _dbContextMock.Setup(x => x.Set<Category, int>()).ReturnsDbSet(CategoryList());
            _dbContextMock.Setup(x => x.Set<TicketStatus, byte>()).ReturnsDbSet(TicketStatusList());
            _userRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(userPerformedAction);

            var ticketService = new TicketService(_dbContextMock.Object, _unitOfWorkMock.Object, _ticketRepositoryMock.Object,
                _userRepositoryMock.Object, _categoryRepositoryMock.Object);

            // Act
            var testResult = await ticketService.GetTicketsAsync(new GetTicketsRequest(page: 1, pageSize: 10), userPerformedAction.Id);

            // Assert
            testResult.Should().NotBeNull();
            testResult.Page.Should().Be(1);
            testResult.PageSize.Should().Be(10);
            testResult.Items.IsNullOrEmpty().Should().BeFalse();
            testResult.TotalCount.Should().Be(TicketList().Count());

            _userRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async Task GetTicketsAsync_Should_ReturnEmptyResponsePageList_WhenNotFoundData()
        {
            // Arrange
            var userPerformedAction = Admin;

            _dbContextMock.Setup(x => x.Set<Ticket, int>()).ReturnsDbSet(Enumerable.Empty<Ticket>());
            _dbContextMock.Setup(x => x.Set<Category, int>()).ReturnsDbSet(Enumerable.Empty<Category>());
            _dbContextMock.Setup(x => x.Set<TicketStatus, byte>()).ReturnsDbSet(Enumerable.Empty<TicketStatus>());
            _userRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(userPerformedAction);

            var ticketService = new TicketService(_dbContextMock.Object, _unitOfWorkMock.Object, _ticketRepositoryMock.Object,
                _userRepositoryMock.Object, _categoryRepositoryMock.Object);

            // Act
            var testResult = await ticketService.GetTicketsAsync(new GetTicketsRequest(page: 1, pageSize: 10), userPerformedAction.Id);

            // Arrange
            testResult.Should().NotBeNull();
            testResult.Page.Should().Be(1);
            testResult.PageSize.Should().Be(10);
            testResult.Items.IsNullOrEmpty().Should().BeTrue();
            testResult.TotalCount.Should().Be(testResult.Items.Count);
        }

        #endregion

        #region Private Methods

        private IEnumerable<TicketTest> TicketList()
        {
            yield return TicketTest.Create(10_100, category: CategoryList().FirstOrDefault(), description: "Lorem ipsum dolor sit amet.", userRequester: UserA);
            yield return TicketTest.Create(10_101, category: CategoryList().FirstOrDefault(), description: "Lorem ipsum dolor sit amet.", userRequester: UserA, userAssigned: AnalystA);

            yield return TicketTest.Create(10_200, category: CategoryList().FirstOrDefault(), description: "Lorem ipsum dolor sit amet.", userRequester: UserB);
            yield return TicketTest.Create(10_201, category: CategoryList().FirstOrDefault(), description: "Lorem ipsum dolor sit amet.", userRequester: UserB, userAssigned: AnalystB);

            yield return TicketTest.Create(10_300, category: CategoryList().FirstOrDefault(), description: "Lorem ipsum dolor sit amet.", userRequester: AnalystA);
            yield return TicketTest.Create(10_301, category: CategoryList().FirstOrDefault(), description: "Lorem ipsum dolor sit amet.", userRequester: AnalystA, userAssigned: AnalystB);

            yield return TicketTest.Create(10_400, category: CategoryList().FirstOrDefault(), description: "Lorem ipsum dolor sit amet.", userRequester: AnalystB);
            yield return TicketTest.Create(10_401, category: CategoryList().FirstOrDefault(), description: "Lorem ipsum dolor sit amet.", userRequester: AnalystB, userAssigned: AnalystA);
        }

        private IEnumerable<TicketStatus> TicketStatusList()
        {
            yield return new TicketStatus(idTicketStatus: (byte)TicketStatuses.New, name: "Novo");
            yield return new TicketStatus(idTicketStatus: (byte)TicketStatuses.Assigned, name: "Atribuído");
            yield return new TicketStatus(idTicketStatus: (byte)TicketStatuses.InProgress, name: "Em andamento");
            yield return new TicketStatus(idTicketStatus: (byte)TicketStatuses.OnHold, name: "Em espera");
            yield return new TicketStatus(idTicketStatus: (byte)TicketStatuses.Completed, name: "Concluído");
            yield return new TicketStatus(idTicketStatus: (byte)TicketStatuses.Cancelled, name: "Cancelado");
        }

        private IEnumerable<Category> CategoryList()
        {
            yield return new Category(idCategory: 1, idPriority: (byte)Priorities.Criticial, name: "Indisponibilidade", description: default);
            yield return new Category(idCategory: 2, idPriority: (byte)Priorities.High, name: "Lentidão", description: default);
            yield return new Category(idCategory: 3, idPriority: (byte)Priorities.Medium, name: "Requisição", description: default);
            yield return new Category(idCategory: 4, idPriority: (byte)Priorities.Low, name: "Dúvida", description: default);
        }

        private UserTest Admin
            => new UserTest(1, "Admin", "Test", Email.Create("admin@test.com"), UserRoles.Administrator, _passwordHasher.HashPassword(Password.Create("Admin@123")));

        private UserTest UserA
            => new UserTest(2, "UserA", "Test", Email.Create("usera@test.com"), UserRoles.General, _passwordHasher.HashPassword(Password.Create("UserA@123")));

        private UserTest UserB
            => new UserTest(3, "UserB", "Test", Email.Create("userb@test.com"), UserRoles.General, _passwordHasher.HashPassword(Password.Create("UserB@123")));

        private UserTest AnalystA
            => new UserTest(4, "UserC", "Test", Email.Create("userc@test.com"), UserRoles.Analyst, _passwordHasher.HashPassword(Password.Create("UserC@123")));

        private UserTest AnalystB
            => new UserTest(5, "UserD", "Test", Email.Create("userd@test.com"), UserRoles.Analyst, _passwordHasher.HashPassword(Password.Create("UserD@123")));

        #endregion
    }
}
