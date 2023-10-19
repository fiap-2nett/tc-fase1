using Moq;
using Xunit;
using System;
using System.Linq;
using FluentAssertions;
using System.Threading;
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

        #region GetTicketByIdAsync

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

        #endregion

        #region GetTicketsAsync

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

        #region CreateAsync

        [Fact]
        public async Task CreateAsync_Should_ThrowNotFoundException_WhenInvalidUserRequester()
        {
            // Arrange
            _userRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((User)null);

            var ticketService = new TicketService(_dbContextMock.Object, _unitOfWorkMock.Object, _ticketRepositoryMock.Object,
                _userRepositoryMock.Object, _categoryRepositoryMock.Object);

            // Act
            var action = () => ticketService.CreateAsync(idCategory: CategoryList().FirstOrDefault().Id,
                description: "Lorem ipsum dolor sit amet.", idUserRequester: int.MaxValue);

            // Assert
            await action.Should()
                .ThrowAsync<NotFoundException>()
                .WithMessage(DomainErrors.User.NotFound.Message);

            _userRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _categoryRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Never);
            _ticketRepositoryMock.Verify(x => x.Insert(It.IsAny<Ticket>()), Times.Never);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CreateAsync_Should_ThrowNotFoundException_WhenInvalidCategory()
        {
            // Arrange
            _userRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(UserA);
            _categoryRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Category)null);

            var ticketService = new TicketService(_dbContextMock.Object, _unitOfWorkMock.Object, _ticketRepositoryMock.Object,
                _userRepositoryMock.Object, _categoryRepositoryMock.Object);

            // Act
            var action = () => ticketService.CreateAsync(idCategory: int.MaxValue,
                description: "Lorem ipsum dolor sit amet.", idUserRequester: UserA.Id);

            // Assert
            await action.Should()
                .ThrowAsync<NotFoundException>()
                .WithMessage(DomainErrors.Category.NotFound.Message);

            _userRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _categoryRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _ticketRepositoryMock.Verify(x => x.Insert(It.IsAny<Ticket>()), Times.Never);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task CreateAsync_Should_ThrowDomainException_WhenDescriptionIsNullOrWhiteSpace(string description)
        {
            // Arrange
            _userRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(UserA);
            _categoryRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(CategoryList().FirstOrDefault());

            var ticketService = new TicketService(_dbContextMock.Object, _unitOfWorkMock.Object, _ticketRepositoryMock.Object,
                _userRepositoryMock.Object, _categoryRepositoryMock.Object);

            // Act
            var action = () => ticketService.CreateAsync(idCategory: CategoryList().FirstOrDefault().Id,
                description, idUserRequester: UserA.Id);

            // Assert
            await action.Should()
                .ThrowAsync<DomainException>()
                .WithMessage(DomainErrors.Ticket.DescriptionIsRequired.Message);

            _userRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _categoryRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _ticketRepositoryMock.Verify(x => x.Insert(It.IsAny<Ticket>()), Times.Never);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CreateAsync_Should_ReturnTicketId_WhenValidParameters()
        {
            // Arrange
            _userRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(UserA);
            _categoryRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(CategoryList().FirstOrDefault());

            var ticketService = new TicketService(_dbContextMock.Object, _unitOfWorkMock.Object, _ticketRepositoryMock.Object,
                _userRepositoryMock.Object, _categoryRepositoryMock.Object);

            // Act
            var testResult = await ticketService.CreateAsync(idCategory: CategoryList().FirstOrDefault().Id,
                description: "Lorem ipsum dolor sit amet.", idUserRequester: UserA.Id);

            // Assert
            testResult.Should().BeGreaterThanOrEqualTo(default);

            _userRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _categoryRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _ticketRepositoryMock.Verify(x => x.Insert(It.IsAny<Ticket>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion

        #region UpdateAsync

        [Fact]
        public async Task UpdateAsync_Should_ThrowNotFoundException_WhenInvalidUserPerformedAction()
        {
            // Arrange
            _userRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((User)null);

            var ticketService = new TicketService(_dbContextMock.Object, _unitOfWorkMock.Object, _ticketRepositoryMock.Object,
                _userRepositoryMock.Object, _categoryRepositoryMock.Object);

            var targetTicket = TicketList().FirstOrDefault();

            // Act
            var action = () => ticketService.UpdateAsync(
                idTicket: targetTicket.Id,
                idCategory: targetTicket.IdCategory,
                description: targetTicket.Description,
                idUserPerformedAction: targetTicket.IdUserRequester);

            // Assert
            await action.Should()
                .ThrowAsync<NotFoundException>()
                .WithMessage(DomainErrors.User.NotFound.Message);

            _userRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _categoryRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Never);
            _ticketRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Never);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_Should_ThrowNotFoundException_WhenInvalidCategory()
        {
            // Arrange
            _userRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(UserA);
            _categoryRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Category)null);

            var ticketService = new TicketService(_dbContextMock.Object, _unitOfWorkMock.Object, _ticketRepositoryMock.Object,
                _userRepositoryMock.Object, _categoryRepositoryMock.Object);

            var targetTicket = TicketList().FirstOrDefault();

            // Act
            var action = () => ticketService.UpdateAsync(
                idTicket: targetTicket.Id,
                idCategory: targetTicket.IdCategory,
                description: targetTicket.Description,
                idUserPerformedAction: targetTicket.IdUserRequester);

            // Assert
            await action.Should()
                .ThrowAsync<NotFoundException>()
                .WithMessage(DomainErrors.Category.NotFound.Message);

            _userRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _categoryRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _ticketRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Never);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_Should_ThrowNotFoundException_WhenInvalidTicket()
        {
            // Arrange
            _userRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(UserA);
            _categoryRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(CategoryList().FirstOrDefault());
            _ticketRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Ticket)null);

            var ticketService = new TicketService(_dbContextMock.Object, _unitOfWorkMock.Object, _ticketRepositoryMock.Object,
                _userRepositoryMock.Object, _categoryRepositoryMock.Object);

            var targetTicket = TicketList().FirstOrDefault();

            // Act
            var action = () => ticketService.UpdateAsync(
                idTicket: int.MaxValue,
                idCategory: targetTicket.IdCategory,
                description: targetTicket.Description,
                idUserPerformedAction: targetTicket.IdUserRequester);

            // Assert
            await action.Should()
                .ThrowAsync<NotFoundException>()
                .WithMessage(DomainErrors.Ticket.NotFound.Message);

            _userRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _categoryRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _ticketRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task UpdateAsync_Should_ThrowDomainException_WhenDescriptionIsNullOrWhiteSpace(string description)
        {
            // Arrange
            var targetTicket = TicketList().FirstOrDefault();

            _userRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(UserA);
            _categoryRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(CategoryList().FirstOrDefault());
            _ticketRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(targetTicket);

            var ticketService = new TicketService(_dbContextMock.Object, _unitOfWorkMock.Object, _ticketRepositoryMock.Object,
                _userRepositoryMock.Object, _categoryRepositoryMock.Object);

            // Act
            var action = () => ticketService.UpdateAsync(
                idTicket: targetTicket.Id,
                idCategory: targetTicket.IdCategory,
                description: description,
                idUserPerformedAction: targetTicket.IdUserRequester);

            // Assert
            await action.Should()
                .ThrowAsync<DomainException>()
                .WithMessage(DomainErrors.Ticket.DescriptionIsRequired.Message);

            _userRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _categoryRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _ticketRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_Should_ThrowDomainException_WhenUserPerformedActionIsNotTheUserRequester()
        {
            // Arrange
            var targetTicket = TicketList().FirstOrDefault();

            _userRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(UserB);
            _categoryRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(CategoryList().FirstOrDefault());
            _ticketRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(targetTicket);

            var ticketService = new TicketService(_dbContextMock.Object, _unitOfWorkMock.Object, _ticketRepositoryMock.Object,
                _userRepositoryMock.Object, _categoryRepositoryMock.Object);

            // Act
            var action = () => ticketService.UpdateAsync(
                idTicket: targetTicket.Id,
                idCategory: targetTicket.IdCategory,
                description: targetTicket.Description,
                idUserPerformedAction: UserB.Id);

            // Assert
            await action.Should()
                .ThrowAsync<InvalidPermissionException>()
                .WithMessage(DomainErrors.User.InvalidPermissions.Message);

            _userRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _categoryRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _ticketRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Theory]
        [InlineData(10_102 /*Completed Ticket*/)]
        [InlineData(10_103 /*Cancelled Ticket*/)]
        public async Task UpdateAsync_Should_ThrowDomainException_WhenTicketStatusIdCompletedOrCancelled(int idTicket)
        {
            // Arrange
            var targetTicket = TicketList().FirstOrDefault(x => x.Id == idTicket);

            _userRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(UserA);
            _categoryRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(CategoryList().FirstOrDefault());
            _ticketRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(targetTicket);

            var ticketService = new TicketService(_dbContextMock.Object, _unitOfWorkMock.Object, _ticketRepositoryMock.Object,
                _userRepositoryMock.Object, _categoryRepositoryMock.Object);

            // Act
            var action = () => ticketService.UpdateAsync(
                idTicket: targetTicket.Id,
                idCategory: targetTicket.IdCategory,
                description: targetTicket.Description,
                idUserPerformedAction: targetTicket.IdUserRequester);

            // Assert
            await action.Should()
                .ThrowAsync<DomainException>()
                .WithMessage(DomainErrors.Ticket.HasAlreadyBeenCompletedOrCancelled.Message);

            _userRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _categoryRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _ticketRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_WhenValidParameters()
        {
            // Arrange
            var targetTicket = TicketList().FirstOrDefault();
            var changedCategory = CategoryList().LastOrDefault();
            var updatedTicketDescription = "Updated ticket description";
            var userPerformedAction = UserList().FirstOrDefault(x => x.Id == targetTicket.IdUserRequester);

            _userRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(userPerformedAction);
            _categoryRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(changedCategory);
            _ticketRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(targetTicket);

            var ticketService = new TicketService(_dbContextMock.Object, _unitOfWorkMock.Object, _ticketRepositoryMock.Object,
                _userRepositoryMock.Object, _categoryRepositoryMock.Object);

            // Act
            await ticketService.UpdateAsync(
                idTicket: targetTicket.Id,
                idCategory: changedCategory.Id,
                description: updatedTicketDescription,
                idUserPerformedAction: userPerformedAction.Id);

            // Assert
            targetTicket.IdCategory.Should().Be(changedCategory.Id);
            targetTicket.Description.Should().Be(updatedTicketDescription);
            targetTicket.LastUpdatedBy.Should().Be(userPerformedAction.Id);

            _userRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _categoryRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _ticketRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion

        #region ChangeStatusAsync

        [Fact]
        public async Task ChangeStatusAsync_Should_ThrowNotFoundException_WhenInvalidUserPerformedAction()
        {
            // Arrange
            _userRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((User)null);

            var ticketService = new TicketService(_dbContextMock.Object, _unitOfWorkMock.Object, _ticketRepositoryMock.Object,
                _userRepositoryMock.Object, _categoryRepositoryMock.Object);

            var targetTicket = TicketList().FirstOrDefault();

            // Act
            var action = () => ticketService.ChangeStatusAsync(
                idTicket: targetTicket.Id,
                changedStatus: TicketStatuses.OnHold,
                idUserPerformedAction: targetTicket.IdUserRequester);

            // Assert
            await action.Should()
                .ThrowAsync<NotFoundException>()
                .WithMessage(DomainErrors.User.NotFound.Message);

            _userRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _ticketRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Never);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ChangeStatusAsync_Should_ThrowNotFoundException_WhenInvalidTicket()
        {
            // Arrange
            _userRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(UserA);
            _ticketRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Ticket)null);

            var ticketService = new TicketService(_dbContextMock.Object, _unitOfWorkMock.Object, _ticketRepositoryMock.Object,
                _userRepositoryMock.Object, _categoryRepositoryMock.Object);

            var targetTicket = TicketList().FirstOrDefault();

            // Act
            var action = () => ticketService.ChangeStatusAsync(
                idTicket: targetTicket.Id,
                changedStatus: TicketStatuses.OnHold,
                idUserPerformedAction: targetTicket.IdUserRequester);

            // Assert
            await action.Should()
                .ThrowAsync<NotFoundException>()
                .WithMessage(DomainErrors.Ticket.NotFound.Message);

            _userRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _ticketRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Theory]
        [InlineData(3 /*General UserRole*/)]
        [InlineData(4 /*Analyst UserRole*/)]
        public async Task ChangeStatusAsync_Should_ThrowInvalidPermissionException_WhenInvalidPerformedUserToThisAction(int idUserPerformedAction)
        {
            // Arrange
            var targetTicket = TicketList().FirstOrDefault();

            _userRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(UserList().FirstOrDefault(x => x.Id == idUserPerformedAction));
            _ticketRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(targetTicket);

            var ticketService = new TicketService(_dbContextMock.Object, _unitOfWorkMock.Object, _ticketRepositoryMock.Object,
                _userRepositoryMock.Object, _categoryRepositoryMock.Object);

            // Act
            var action = () => ticketService.ChangeStatusAsync(
                idTicket: targetTicket.Id,
                changedStatus: TicketStatuses.OnHold,
                idUserPerformedAction: targetTicket.IdUserRequester);

            // Assert
            await action.Should()
                .ThrowAsync<InvalidPermissionException>()
                .WithMessage(DomainErrors.Ticket.StatusCannotBeChangedByThisUser.Message);

            _userRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _ticketRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ChangeStatusAsync_Should_ThrowDomainException_WhenChangeStatusToNew()
        {
            // Arrange
            var targetTicket = TicketList().FirstOrDefault();

            _userRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(UserList().FirstOrDefault(x => x.Id == Admin.Id));
            _ticketRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(targetTicket);

            var ticketService = new TicketService(_dbContextMock.Object, _unitOfWorkMock.Object, _ticketRepositoryMock.Object,
                _userRepositoryMock.Object, _categoryRepositoryMock.Object);

            // Act
            var action = () => ticketService.ChangeStatusAsync(
                idTicket: targetTicket.Id,
                changedStatus: TicketStatuses.New,
                idUserPerformedAction: Admin.Id);

            // Assert
            await action.Should()
                .ThrowAsync<DomainException>()
                .WithMessage(DomainErrors.Ticket.CannotChangeStatusToNew.Message);

            _userRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _ticketRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Theory]
        [InlineData(10_102 /*Completed Ticket*/)]
        [InlineData(10_103 /*Cancelled Ticket*/)]
        public async Task ChangeStatusAsync_Should_ThrowDomainException_WhenHasAlreadyBeenCompletedOrCancelled(int idTicket)
        {
            // Arrange
            var targetTicket = TicketList().FirstOrDefault(x => x.Id == idTicket);
            var userAssigned = UserList().FirstOrDefault(x => x.Id == targetTicket.IdUserAssigned);

            _userRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(userAssigned);
            _ticketRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(targetTicket);

            var ticketService = new TicketService(_dbContextMock.Object, _unitOfWorkMock.Object, _ticketRepositoryMock.Object,
                _userRepositoryMock.Object, _categoryRepositoryMock.Object);

            // Act
            var action = () => ticketService.ChangeStatusAsync(
                idTicket: targetTicket.Id,
                changedStatus: TicketStatuses.OnHold,
                idUserPerformedAction: userAssigned.Id);

            // Assert
            await action.Should()
                .ThrowAsync<DomainException>()
                .WithMessage(DomainErrors.Ticket.HasAlreadyBeenCompletedOrCancelled.Message);

            _userRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _ticketRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Theory]
        [InlineData(TicketStatuses.Assigned)]
        [InlineData(TicketStatuses.Cancelled)]
        public async Task ChangeStatusAsync_Should_ThrowDomainException_WhenHasStatusNotAllowed(TicketStatuses changedStatus)
        {
            // Arrange
            var targetTicket = TicketList().FirstOrDefault(x => x.IdStatus == (byte)TicketStatuses.Assigned);
            var userAssigned = UserList().FirstOrDefault(x => x.Id == targetTicket.IdUserAssigned);

            _userRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(userAssigned);
            _ticketRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(targetTicket);

            var ticketService = new TicketService(_dbContextMock.Object, _unitOfWorkMock.Object, _ticketRepositoryMock.Object,
                _userRepositoryMock.Object, _categoryRepositoryMock.Object);

            // Act
            var action = () => ticketService.ChangeStatusAsync(
                idTicket: targetTicket.Id,
                changedStatus: changedStatus,
                idUserPerformedAction: userAssigned.Id);

            // Assert
            await action.Should()
                .ThrowAsync<DomainException>()
                .WithMessage(DomainErrors.Ticket.StatusNotAllowed.Message);

            _userRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _ticketRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ChangeStatusAsync_Should_Return_WhenValidParameters()
        {
            // Arrange
            var targetTicket = TicketList().FirstOrDefault(x => x.IdStatus == (byte)TicketStatuses.Assigned);
            var userAssigned = UserList().FirstOrDefault(x => x.Id == targetTicket.IdUserAssigned);

            _userRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(userAssigned);
            _ticketRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(targetTicket);

            var ticketService = new TicketService(_dbContextMock.Object, _unitOfWorkMock.Object, _ticketRepositoryMock.Object,
                _userRepositoryMock.Object, _categoryRepositoryMock.Object);

            // Act
            await ticketService.ChangeStatusAsync(
                idTicket: targetTicket.Id,
                changedStatus: TicketStatuses.InProgress,
                idUserPerformedAction: userAssigned.Id);

            // Assert
            targetTicket.LastUpdatedBy.Should().Be(userAssigned.Id);
            targetTicket.IdStatus.Should().Be((byte)TicketStatuses.InProgress);

            _userRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _ticketRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion

        #region AssignToUserAsync

        [Fact]
        public async Task AssignToUserAsync_Should_ThrowNotFoundException_WhenInvalidUserAssigned()
        {
            // Arrange
            var targetTicket = TicketList().FirstOrDefault();
            var idUserAssigned = int.MaxValue;
            var idUserPerformedAction = Admin.Id;

            _userRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((int idUser) => UserList().FirstOrDefault(x => x.Id == idUser));

            var ticketService = new TicketService(_dbContextMock.Object, _unitOfWorkMock.Object, _ticketRepositoryMock.Object,
                _userRepositoryMock.Object, _categoryRepositoryMock.Object);

            // Act
            var action = () => ticketService.AssignToUserAsync(targetTicket.Id, idUserAssigned, idUserPerformedAction);

            // Assert
            await action.Should()
                .ThrowAsync<NotFoundException>()
                .WithMessage(DomainErrors.User.NotFound.Message);

            _userRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _ticketRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Never);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task AssignToUserAsync_Should_ThrowNotFoundException_WhenInvalidUserPerformedAction()
        {
            // Arrange
            var targetTicket = TicketList().FirstOrDefault();
            var idUserAssigned = AnalystA.Id;
            var idUserPerformedAction = int.MaxValue;

            _userRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((int idUser) => UserList().FirstOrDefault(x => x.Id == idUser));

            var ticketService = new TicketService(_dbContextMock.Object, _unitOfWorkMock.Object, _ticketRepositoryMock.Object,
                _userRepositoryMock.Object, _categoryRepositoryMock.Object);

            // Act
            var action = () => ticketService.AssignToUserAsync(targetTicket.Id, idUserAssigned, idUserPerformedAction);

            // Assert
            await action.Should()
                .ThrowAsync<NotFoundException>()
                .WithMessage(DomainErrors.User.NotFound.Message);

            _userRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Exactly(2));
            _ticketRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Never);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task AssignToUserAsync_Should_ThrowNotFoundException_WhenInvalidTicketId()
        {
            // Arrange
            var targetTicket = TicketList().FirstOrDefault();
            var idUserAssigned = AnalystA.Id;
            var idUserPerformedAction = AnalystA.Id;

            _userRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((int idUser) => UserList().FirstOrDefault(x => x.Id == idUser));
            _ticketRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Ticket)null);

            var ticketService = new TicketService(_dbContextMock.Object, _unitOfWorkMock.Object, _ticketRepositoryMock.Object,
                _userRepositoryMock.Object, _categoryRepositoryMock.Object);

            // Act
            var action = () => ticketService.AssignToUserAsync(targetTicket.Id, idUserAssigned, idUserPerformedAction);

            // Assert
            await action.Should()
                .ThrowAsync<NotFoundException>()
                .WithMessage(DomainErrors.Ticket.NotFound.Message);

            _userRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Exactly(2));
            _ticketRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task AssignToUserAsync_Should_ThrowInvalidPermissionException_WhenAssignedUserIsNotAnalyst()
        {
            // Arrange
            var targetTicket = TicketList().FirstOrDefault();
            var idUserAssigned = UserB.Id;
            var idUserPerformedAction = Admin.Id;

            _userRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((int idUser) => UserList().FirstOrDefault(x => x.Id == idUser));
            _ticketRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(targetTicket);

            var ticketService = new TicketService(_dbContextMock.Object, _unitOfWorkMock.Object, _ticketRepositoryMock.Object,
                _userRepositoryMock.Object, _categoryRepositoryMock.Object);

            // Act
            var action = () => ticketService.AssignToUserAsync(targetTicket.Id, idUserAssigned, idUserPerformedAction);

            // Assert
            await action.Should()
                .ThrowAsync<InvalidPermissionException>()
                .WithMessage(DomainErrors.Ticket.CannotBeAssignedToThisUser.Message);

            _userRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Exactly(2));
            _ticketRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task AssignToUserAsync_Should_ThrowInvalidPermissionException_WhenUserPerformedActionIsGeneral()
        {
            // Arrange
            var targetTicket = TicketList().FirstOrDefault();
            var idUserAssigned = AnalystA.Id;
            var idUserPerformedAction = targetTicket.IdUserRequester;

            _userRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((int idUser) => UserList().FirstOrDefault(x => x.Id == idUser));
            _ticketRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(targetTicket);

            var ticketService = new TicketService(_dbContextMock.Object, _unitOfWorkMock.Object, _ticketRepositoryMock.Object,
                _userRepositoryMock.Object, _categoryRepositoryMock.Object);

            // Act
            var action = () => ticketService.AssignToUserAsync(targetTicket.Id, idUserAssigned, idUserPerformedAction);

            // Assert
            await action.Should()
                .ThrowAsync<InvalidPermissionException>()
                .WithMessage(DomainErrors.User.InvalidPermissions.Message);

            _userRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Exactly(2));
            _ticketRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Theory]
        [InlineData(10_102 /*Completed Ticket*/)]
        [InlineData(10_103 /*Cancelled Ticket*/)]
        public async Task AssignToUserAsync_Should_ThrowInvalidPermissionException_WhenTicketHasAlreadyBeenCompletedOrCancelledl(int idTicket)
        {
            // Arrange
            var targetTicket = TicketList().FirstOrDefault(x => x.Id == idTicket);
            var idUserAssigned = AnalystA.Id;
            var idUserPerformedAction = AnalystA.Id;

            _userRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((int idUser) => UserList().FirstOrDefault(x => x.Id == idUser));
            _ticketRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(targetTicket);

            var ticketService = new TicketService(_dbContextMock.Object, _unitOfWorkMock.Object, _ticketRepositoryMock.Object,
                _userRepositoryMock.Object, _categoryRepositoryMock.Object);

            // Act
            var action = () => ticketService.AssignToUserAsync(targetTicket.Id, idUserAssigned, idUserPerformedAction);

            // Assert
            await action.Should()
                .ThrowAsync<DomainException>()
                .WithMessage(DomainErrors.Ticket.HasAlreadyBeenCompletedOrCancelled.Message);

            _userRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Exactly(2));
            _ticketRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task AssignToUserAsync_Should_ThrowInvalidPermissionException_WhenTicketStatusIsNewAndUserPerformedActionIsNotAdministratorOrAssignedUser()
        {
            // Arrange
            var targetTicket = TicketList().FirstOrDefault();
            var idUserAssigned = AnalystA.Id;
            var idUserPerformedAction = AnalystB.Id;

            _userRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((int idUser) => UserList().FirstOrDefault(x => x.Id == idUser));
            _ticketRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(targetTicket);

            var ticketService = new TicketService(_dbContextMock.Object, _unitOfWorkMock.Object, _ticketRepositoryMock.Object,
                _userRepositoryMock.Object, _categoryRepositoryMock.Object);

            // Act
            var action = () => ticketService.AssignToUserAsync(targetTicket.Id, idUserAssigned, idUserPerformedAction);

            // Assert
            await action.Should()
                .ThrowAsync<InvalidPermissionException>()
                .WithMessage(DomainErrors.User.InvalidPermissions.Message);

            _userRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Exactly(2));
            _ticketRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task AssignToUserAsync_Should_ThrowInvalidPermissionException_WhenTicketStatusIsAssignedAndUserPerformedActionIsNotAdministratorOrAssignedUser()
        {
            // Arrange
            var targetTicket = TicketList().FirstOrDefault(x => x.IdStatus == (byte)TicketStatuses.Assigned);
            var idUserAssigned = AnalystB.Id;
            var idUserPerformedAction = AnalystB.Id;

            _userRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((int idUser) => UserList().FirstOrDefault(x => x.Id == idUser));
            _ticketRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(targetTicket);

            var ticketService = new TicketService(_dbContextMock.Object, _unitOfWorkMock.Object, _ticketRepositoryMock.Object,
                _userRepositoryMock.Object, _categoryRepositoryMock.Object);

            // Act
            var action = () => ticketService.AssignToUserAsync(targetTicket.Id, idUserAssigned, idUserPerformedAction);

            // Assert
            await action.Should()
                .ThrowAsync<InvalidPermissionException>()
                .WithMessage(DomainErrors.User.InvalidPermissions.Message);

            _userRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Exactly(2));
            _ticketRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task AssignToUserAsync_Should_Return_WhenValidParameters()
        {
            // Arrange
            var targetTicket = TicketList().FirstOrDefault();
            var idUserAssigned = AnalystA.Id;
            var idUserPerformedAction = Admin.Id;

            _userRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((int idUser) => UserList().FirstOrDefault(x => x.Id == idUser));
            _ticketRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(targetTicket);

            var ticketService = new TicketService(_dbContextMock.Object, _unitOfWorkMock.Object, _ticketRepositoryMock.Object,
                _userRepositoryMock.Object, _categoryRepositoryMock.Object);

            // Act
            await ticketService.AssignToUserAsync(targetTicket.Id, idUserAssigned, idUserPerformedAction);

            // Assert
            targetTicket.IdUserAssigned.Should().Be(idUserAssigned);
            targetTicket.LastUpdatedBy.Should().Be(idUserPerformedAction);
            targetTicket.IdStatus.Should().Be((byte)TicketStatuses.Assigned);

            _userRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Exactly(2));
            _ticketRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion

        #region CompleteAsync

        [Fact]
        public async Task CompleteAsync_Should_ThrowNotFoundException_WhenInvalidUserPerformendAction()
        {
            // Arrange
            var targetTicket = TicketList().FirstOrDefault();
            var idUserPerformedAction = int.MaxValue;

            _userRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((int idUser) => UserList().FirstOrDefault(x => x.Id == idUser));

            var ticketService = new TicketService(_dbContextMock.Object, _unitOfWorkMock.Object, _ticketRepositoryMock.Object,
                _userRepositoryMock.Object, _categoryRepositoryMock.Object);

            // Act
            var action = () => ticketService.CompleteAsync(targetTicket.Id, idUserPerformedAction);

            // Assert
            await action.Should()
                .ThrowAsync<NotFoundException>()
                .WithMessage(DomainErrors.User.NotFound.Message);

            _userRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _ticketRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Never);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CompleteAsync_Should_ThrowNotFoundException_WhenInvalidTicketId()
        {
            // Arrange
            var targetTicket = TicketList().FirstOrDefault();
            var idUserPerformedAction = AnalystA.Id;

            _userRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((int idUser) => UserList().FirstOrDefault(x => x.Id == idUser));
            _ticketRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Ticket)null);

            var ticketService = new TicketService(_dbContextMock.Object, _unitOfWorkMock.Object, _ticketRepositoryMock.Object,
                _userRepositoryMock.Object, _categoryRepositoryMock.Object);

            // Act
            var action = () => ticketService.CompleteAsync(targetTicket.Id, idUserPerformedAction);

            // Assert
            await action.Should()
                .ThrowAsync<NotFoundException>()
                .WithMessage(DomainErrors.Ticket.NotFound.Message);

            _userRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _ticketRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CompleteAsync_Should_ThrowInvalidPermissionException_WhenUserPerformedActionIsGeneral()
        {
            // Arrange
            var targetTicket = TicketList().FirstOrDefault();
            var idUserPerformedAction = targetTicket.IdUserRequester;

            _userRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((int idUser) => UserList().FirstOrDefault(x => x.Id == idUser));
            _ticketRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(targetTicket);

            var ticketService = new TicketService(_dbContextMock.Object, _unitOfWorkMock.Object, _ticketRepositoryMock.Object,
                _userRepositoryMock.Object, _categoryRepositoryMock.Object);

            // Act
            var action = () => ticketService.CompleteAsync(targetTicket.Id, idUserPerformedAction);

            // Assert
            await action.Should()
                .ThrowAsync<InvalidPermissionException>()
                .WithMessage(DomainErrors.Ticket.CannotBeCompletedByThisUser.Message);

            _userRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _ticketRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CompleteAsync_Should_ThrowInvalidPermissionException_WhenUserPerformedActionIsNotAssignedUser()
        {
            // Arrange
            var targetTicket = TicketList().FirstOrDefault(x => x.IdStatus == (byte)TicketStatuses.Assigned);
            var idUserPerformedAction = AnalystB.Id;

            _userRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((int idUser) => UserList().FirstOrDefault(x => x.Id == idUser));
            _ticketRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(targetTicket);

            var ticketService = new TicketService(_dbContextMock.Object, _unitOfWorkMock.Object, _ticketRepositoryMock.Object,
                _userRepositoryMock.Object, _categoryRepositoryMock.Object);

            // Act
            var action = () => ticketService.CompleteAsync(targetTicket.Id, idUserPerformedAction);

            // Assert
            await action.Should()
                .ThrowAsync<InvalidPermissionException>()
                .WithMessage(DomainErrors.Ticket.CannotBeCompletedByThisUser.Message);

            _userRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _ticketRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CompleteAsync_Should_ThrowDomainException_WhenTicketStatusIsNew()
        {
            // Arrange
            var targetTicket = TicketList().FirstOrDefault();
            var idUserPerformedAction = Admin.Id;

            _userRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((int idUser) => UserList().FirstOrDefault(x => x.Id == idUser));
            _ticketRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(targetTicket);

            var ticketService = new TicketService(_dbContextMock.Object, _unitOfWorkMock.Object, _ticketRepositoryMock.Object,
                _userRepositoryMock.Object, _categoryRepositoryMock.Object);

            // Act
            var action = () => ticketService.CompleteAsync(targetTicket.Id, idUserPerformedAction);

            // Assert
            await action.Should()
                .ThrowAsync<DomainException>()
                .WithMessage(DomainErrors.Ticket.HasNotBeenAssignedToAUser.Message);

            _userRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _ticketRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Theory]
        [InlineData(10_102 /*Completed Ticket*/)]
        [InlineData(10_103 /*Cancelled Ticket*/)]
        public async Task CompleteAsync_Should_ThrowDomainException_WhenTicketStatusIsCompletedOrCancelled(int idTicket)
        {
            // Arrange
            var targetTicket = TicketList().FirstOrDefault(x => x.Id == idTicket);
            var idUserPerformedAction = AnalystA.Id;

            _userRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((int idUser) => UserList().FirstOrDefault(x => x.Id == idUser));
            _ticketRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(targetTicket);

            var ticketService = new TicketService(_dbContextMock.Object, _unitOfWorkMock.Object, _ticketRepositoryMock.Object,
                _userRepositoryMock.Object, _categoryRepositoryMock.Object);

            // Act
            var action = () => ticketService.CompleteAsync(targetTicket.Id, idUserPerformedAction);

            // Assert
            await action.Should()
                .ThrowAsync<DomainException>()
                .WithMessage(DomainErrors.Ticket.HasAlreadyBeenCompletedOrCancelled.Message);

            _userRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _ticketRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CompleteAsync_Should_Return_WhenValidParameters()
        {
            // Arrange
            var targetTicket = TicketList().FirstOrDefault(x =>x.IdStatus == (byte)TicketStatuses.Assigned);
            var idUserPerformedAction = AnalystA.Id;

            _userRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((int idUser) => UserList().FirstOrDefault(x => x.Id == idUser));
            _ticketRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(targetTicket);

            var ticketService = new TicketService(_dbContextMock.Object, _unitOfWorkMock.Object, _ticketRepositoryMock.Object,
                _userRepositoryMock.Object, _categoryRepositoryMock.Object);

            // Act
            await ticketService.CompleteAsync(targetTicket.Id, idUserPerformedAction);

            // Assert
            targetTicket.CompletedAt.Should().NotBeNull();
            targetTicket.LastUpdatedBy.Should().Be(idUserPerformedAction);
            targetTicket.IdStatus.Should().Be((byte)TicketStatuses.Completed);

            _userRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _ticketRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion

        #region CancelAsync

        [Fact]
        public async Task CancelAsync_Should_ThrowNotFoundException_WhenInvalidUserPerformendAction()
        {
            // Arrange
            var targetTicket = TicketList().FirstOrDefault();
            var idUserPerformedAction = int.MaxValue;
            var cancellationReason = "Lorem ipsum dolor sit amet.";

            _userRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((int idUser) => UserList().FirstOrDefault(x => x.Id == idUser));

            var ticketService = new TicketService(_dbContextMock.Object, _unitOfWorkMock.Object, _ticketRepositoryMock.Object,
                _userRepositoryMock.Object, _categoryRepositoryMock.Object);

            // Act
            var action = () => ticketService.CancelAsync(targetTicket.Id, cancellationReason, idUserPerformedAction);

            // Assert
            await action.Should()
                .ThrowAsync<NotFoundException>()
                .WithMessage(DomainErrors.User.NotFound.Message);

            _userRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _ticketRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Never);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CancelAsync_Should_ThrowNotFoundException_WhenInvalidTicketId()
        {
            // Arrange
            var targetTicket = TicketList().FirstOrDefault();
            var idUserPerformedAction = AnalystA.Id;
            var cancellationReason = "Lorem ipsum dolor sit amet.";

            _userRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((int idUser) => UserList().FirstOrDefault(x => x.Id == idUser));
            _ticketRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Ticket)null);

            var ticketService = new TicketService(_dbContextMock.Object, _unitOfWorkMock.Object, _ticketRepositoryMock.Object,
                _userRepositoryMock.Object, _categoryRepositoryMock.Object);

            // Act
            var action = () => ticketService.CancelAsync(targetTicket.Id, cancellationReason, idUserPerformedAction);

            // Assert
            await action.Should()
                .ThrowAsync<NotFoundException>()
                .WithMessage(DomainErrors.Ticket.NotFound.Message);

            _userRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _ticketRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CancelAsync_Should_ThrowInvalidPermissionException_WhenUserPerformedActionIsGeneralAndNotUserRequester()
        {
            // Arrange
            var targetTicket = TicketList().FirstOrDefault();
            var idUserPerformedAction = UserB.Id;
            var cancellationReason = "Lorem ipsum dolor sit amet.";

            _userRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((int idUser) => UserList().FirstOrDefault(x => x.Id == idUser));
            _ticketRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(targetTicket);

            var ticketService = new TicketService(_dbContextMock.Object, _unitOfWorkMock.Object, _ticketRepositoryMock.Object,
                _userRepositoryMock.Object, _categoryRepositoryMock.Object);

            // Act
            var action = () => ticketService.CancelAsync(targetTicket.Id, cancellationReason, idUserPerformedAction);

            // Assert
            await action.Should()
                .ThrowAsync<InvalidPermissionException>()
                .WithMessage(DomainErrors.Ticket.CannotBeCancelledByThisUser.Message);

            _userRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _ticketRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CancelAsync_Should_ThrowInvalidPermissionException_WhenUserPerformedActionIsAnalystAndNotUserRequesterOrUserAssigned()
        {
            // Arrange
            var targetTicket = TicketList().FirstOrDefault(x => x.IdStatus == (byte)TicketStatuses.Assigned);
            var idUserPerformedAction = AnalystB.Id;
            var cancellationReason = "Lorem ipsum dolor sit amet.";

            _userRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((int idUser) => UserList().FirstOrDefault(x => x.Id == idUser));
            _ticketRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(targetTicket);

            var ticketService = new TicketService(_dbContextMock.Object, _unitOfWorkMock.Object, _ticketRepositoryMock.Object,
                _userRepositoryMock.Object, _categoryRepositoryMock.Object);

            // Act
            var action = () => ticketService.CancelAsync(targetTicket.Id, cancellationReason, idUserPerformedAction);

            // Assert
            await action.Should()
                .ThrowAsync<InvalidPermissionException>()
                .WithMessage(DomainErrors.Ticket.CannotBeCancelledByThisUser.Message);

            _userRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _ticketRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task CancelAsync_Should_ThrowDomainException_WhenCancellationReasonIsNullOrWhiteSpace(string cancellationReason)
        {
            // Arrange
            var targetTicket = TicketList().FirstOrDefault(x => x.IdStatus == (byte)TicketStatuses.Assigned);
            var idUserPerformedAction = targetTicket.IdUserRequester;

            _userRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((int idUser) => UserList().FirstOrDefault(x => x.Id == idUser));
            _ticketRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(targetTicket);

            var ticketService = new TicketService(_dbContextMock.Object, _unitOfWorkMock.Object, _ticketRepositoryMock.Object,
                _userRepositoryMock.Object, _categoryRepositoryMock.Object);

            // Act
            var action = () => ticketService.CancelAsync(targetTicket.Id, cancellationReason, idUserPerformedAction);

            // Assert
            await action.Should()
                .ThrowAsync<DomainException>()
                .WithMessage(DomainErrors.Ticket.CancellationReasonIsRequired.Message);

            _userRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _ticketRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Theory]
        [InlineData(10_102 /*Completed Ticket*/)]
        [InlineData(10_103 /*Cancelled Ticket*/)]
        public async Task CancelAsync_Should_ThrowDomainException_WhenTicketStatusIsCompletedOrCancelled(int idTicket)
        {
            // Arrange
            var targetTicket = TicketList().FirstOrDefault(x => x.Id == idTicket);
            var idUserPerformedAction = targetTicket.IdUserRequester;
            var cancellationReason = "Lorem ipsum dolor sit amet.";

            _userRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((int idUser) => UserList().FirstOrDefault(x => x.Id == idUser));
            _ticketRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(targetTicket);

            var ticketService = new TicketService(_dbContextMock.Object, _unitOfWorkMock.Object, _ticketRepositoryMock.Object,
                _userRepositoryMock.Object, _categoryRepositoryMock.Object);

            // Act
            var action = () => ticketService.CancelAsync(targetTicket.Id, cancellationReason, idUserPerformedAction);

            // Assert
            await action.Should()
                .ThrowAsync<DomainException>()
                .WithMessage(DomainErrors.Ticket.HasAlreadyBeenCompletedOrCancelled.Message);

            _userRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _ticketRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CancelAsync_Should_Return_WhenValidParameters()
        {
            // Arrange
            var targetTicket = TicketList().FirstOrDefault(x => x.IdStatus == (byte)TicketStatuses.Assigned);
            var idUserPerformedAction = AnalystA.Id;
            var cancellationReason = "Lorem ipsum dolor sit amet.";

            _userRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((int idUser) => UserList().FirstOrDefault(x => x.Id == idUser));
            _ticketRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(targetTicket);

            var ticketService = new TicketService(_dbContextMock.Object, _unitOfWorkMock.Object, _ticketRepositoryMock.Object,
                _userRepositoryMock.Object, _categoryRepositoryMock.Object);

            // Act
            await ticketService.CancelAsync(targetTicket.Id, cancellationReason, idUserPerformedAction);

            // Assert
            targetTicket.CompletedAt.Should().BeNull();
            targetTicket.CancellationReason.Should().Be(cancellationReason);
            targetTicket.LastUpdatedBy.Should().Be(idUserPerformedAction);
            targetTicket.IdStatus.Should().Be((byte)TicketStatuses.Cancelled);

            _userRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _ticketRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion

        #endregion

        #region Private Methods

        private IEnumerable<TicketTest> TicketList()
        {
            yield return TicketTest.Create(10_100, category: CategoryList().FirstOrDefault(), description: "Lorem ipsum dolor sit amet.", userRequester: UserA);
            yield return TicketTest.Create(10_101, category: CategoryList().FirstOrDefault(), description: "Lorem ipsum dolor sit amet.", userRequester: UserA, userAssigned: AnalystA);
            yield return TicketTest.Create(10_102, category: CategoryList().FirstOrDefault(), description: "Lorem ipsum dolor sit amet.", userRequester: UserA, userAssigned: AnalystA, toComplete: true);
            yield return TicketTest.Create(10_103, category: CategoryList().FirstOrDefault(), description: "Lorem ipsum dolor sit amet.", userRequester: UserA, userAssigned: AnalystA, toCancelled: true);

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

        private IEnumerable<User> UserList()
        {
            yield return Admin;
            yield return UserA;
            yield return UserB;
            yield return AnalystA;
            yield return AnalystB;
        }

        #endregion
    }
}
