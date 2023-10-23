using Moq;
using Xunit;
using System;
using System.Linq;
using FluentAssertions;
using System.Threading.Tasks;
using Moq.EntityFrameworkCore;
using System.Collections.Generic;
using TechChallenge.Domain.Errors;
using Microsoft.Extensions.Options;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Extensions;
using TechChallenge.Domain.Exceptions;
using TechChallenge.Domain.Enumerations;
using TechChallenge.Domain.ValueObjects;
using TechChallenge.Domain.Repositories;
using TechChallenge.Application.Services;
using TechChallenge.Application.Contracts.Users;
using TechChallenge.Infrastructure.Cryptography;
using TechChallenge.Infrastructure.Authentication;
using TechChallenge.Application.UnitTests.TestEntities;
using TechChallenge.Application.Core.Abstractions.Data;
using TechChallenge.Infrastructure.Authentication.Settings;
using TechChallenge.Application.Core.Abstractions.Cryptography;
using TechChallenge.Application.Core.Abstractions.Authentication;

namespace TechChallenge.Application.UnitTests.Scenarios
{
    public sealed class UserServiceTests
    {
        #region Read-Only Fields

        private readonly IJwtProvider _jwtProvider;
        private readonly IPasswordHasher _passwordHasher;

        private readonly Mock<IDbContext> _dbContextMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;

        #endregion

        #region Constructors

        public UserServiceTests()
        {
            _dbContextMock = new();
            _unitOfWorkMock = new();
            _userRepositoryMock = new();

            _passwordHasher = new PasswordHasher();
            _jwtProvider = new JwtProvider(JwtOptions);
        }

        #endregion

        #region Unit Tests

        #region CreateAsync

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task CreateAsync_Should_ThrowArgumentException_WhenEmailIsNullOrWhiteSpace(string email)
        {
            // Arrange
            var userService = new UserService(_dbContextMock.Object, _unitOfWorkMock.Object, _jwtProvider,
                _userRepositoryMock.Object, _passwordHasher);

            // Act
            var action = () => userService.CreateAsync("John", "Doe", email, "John@123");

            // Assert
            await action.Should()
                .ThrowAsync<ArgumentException>()
                .WithParameterName(nameof(email))
                .WithMessage(new ArgumentException(DomainErrors.Email.NullOrEmpty.Message, nameof(email)).Message);

            _userRepositoryMock.Verify(x => x.IsEmailUniqueAsync(It.IsAny<Email>()), Times.Never());
            _userRepositoryMock.Verify(x => x.Insert(It.IsAny<User>()), Times.Never());
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(default), Times.Never());
        }

        [Fact]
        public async Task CreateAsync_Should_ThrowArgumentException_WhenEmailIsLongerThanAllowed()
        {
            // Arrange
            var email = $"{new string('a', Email.MaxLength)}@test.com";

            var userService = new UserService(_dbContextMock.Object, _unitOfWorkMock.Object, _jwtProvider,
                _userRepositoryMock.Object, _passwordHasher);

            // Act
            var action = () => userService.CreateAsync("John", "Doe", email, "John@123");

            // Assert
            await action.Should()
                .ThrowAsync<ArgumentException>()
                .WithParameterName(nameof(email))
                .WithMessage(new ArgumentException(DomainErrors.Email.LongerThanAllowed.Message, nameof(email)).Message);

            _userRepositoryMock.Verify(x => x.IsEmailUniqueAsync(It.IsAny<Email>()), Times.Never());
            _userRepositoryMock.Verify(x => x.Insert(It.IsAny<User>()), Times.Never());
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(default), Times.Never());
        }

        [Theory]
        [InlineData("john.doe@test")]
        [InlineData("john.doe@test.")]
        [InlineData("john.doe@test.a")]
        public async Task CreateAsync_Should_ThrowArgumentException_WhenEmailIsInvalidFormat(string email)
        {
            // Arrange
            var userService = new UserService(_dbContextMock.Object, _unitOfWorkMock.Object, _jwtProvider,
                _userRepositoryMock.Object, _passwordHasher);

            // Act
            var action = () => userService.CreateAsync("John", "Doe", email, "John@123");

            // Assert
            await action.Should()
                .ThrowAsync<ArgumentException>()
                .WithParameterName(nameof(email))
                .WithMessage(new ArgumentException(DomainErrors.Email.InvalidFormat.Message, nameof(email)).Message);

            _userRepositoryMock.Verify(x => x.IsEmailUniqueAsync(It.IsAny<Email>()), Times.Never());
            _userRepositoryMock.Verify(x => x.Insert(It.IsAny<User>()), Times.Never());
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(default), Times.Never());
        }

        [Fact]
        public async Task CreateAsync_Should_ThrowDomainException_WhenEmailIsNotUnique()
        {
            // Arrange
            _userRepositoryMock.Setup(x => x.IsEmailUniqueAsync(It.IsAny<Email>())).ReturnsAsync(false);

            var userService = new UserService(_dbContextMock.Object, _unitOfWorkMock.Object, _jwtProvider,
                _userRepositoryMock.Object, _passwordHasher);

            // Act
            var action = () => userService.CreateAsync("John", "Doe", "john.doe@test.com", "John@123");

            // Assert
            await action.Should()
                .ThrowAsync<DomainException>()
                .WithMessage(DomainErrors.User.DuplicateEmail.Message);

            _userRepositoryMock.Verify(x => x.Insert(It.IsAny<User>()), Times.Never());
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(default), Times.Never());
        }

        [Theory]
        [InlineData("", "")]
        [InlineData(" ", " ")]
        [InlineData(null, null)]
        public async Task CreateAsync_Should_ThrowDomainException_WhenRequiredValuesAreInvalid(string name, string surname)
        {
            // Arrange
            _userRepositoryMock.Setup(x => x.IsEmailUniqueAsync(It.IsAny<Email>())).ReturnsAsync(true);

            var userService = new UserService(_dbContextMock.Object, _unitOfWorkMock.Object, _jwtProvider,
                _userRepositoryMock.Object, _passwordHasher);

            // Act
            var action = () => userService.CreateAsync(name, surname, "john.doe@test.com", "John@123");

            // Assert
            await action.Should()
                .ThrowAsync<ArgumentException>();

            _userRepositoryMock.Verify(x => x.Insert(It.IsAny<User>()), Times.Never());
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(default), Times.Never());
        }

        [Fact]
        public async Task CreateAsync_Should_ReturnTokenResponse_WhenValidUserData()
        {
            // Arrange
            _userRepositoryMock.Setup(x => x.IsEmailUniqueAsync(It.IsAny<Email>())).ReturnsAsync(true);

            var userService = new UserService(_dbContextMock.Object, _unitOfWorkMock.Object, _jwtProvider,
                _userRepositoryMock.Object, _passwordHasher);

            // Act
            var testResult = await userService.CreateAsync("John", "Doe", "john.doe@test.com", "John@123");

            // Assert
            testResult.Should().NotBeNull();
            testResult.Token.Should().NotBeNullOrWhiteSpace();

            _userRepositoryMock.Verify(x => x.IsEmailUniqueAsync(It.IsAny<Email>()), Times.Once);
            _userRepositoryMock.Verify(x => x.Insert(It.IsAny<User>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(default), Times.Once);
        }

        #endregion

        #region ChangePasswordAsync

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        [InlineData("John@")]
        [InlineData("JOHN@123")]
        [InlineData("John@Doe")]
        [InlineData("JohnDoe")]
        public async Task ChangePasswordAsync_Should_ThrowArgumentException_WhenInvalidPassword(string password)
        {
            // Arrange
            var userService = new UserService(_dbContextMock.Object, _unitOfWorkMock.Object, _jwtProvider,
                _userRepositoryMock.Object, _passwordHasher);

            // Act
            var action = () => userService.ChangePasswordAsync(1, password);

            // Assert
            await action.Should()
                .ThrowAsync<ArgumentException>()
                .WithParameterName(nameof(password));

            _userRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Never);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(default), Times.Never());
        }

        [Fact]
        public async Task ChangePasswordAsync_Should_ThrowDomainException_WhenInvalidIdUser()
        {
            // Arrange
            _userRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((User)null);

            var userService = new UserService(_dbContextMock.Object, _unitOfWorkMock.Object, _jwtProvider,
                _userRepositoryMock.Object, _passwordHasher);

            // Act
            var action = () => userService.ChangePasswordAsync(1, "John@123");

            // Assert
            await action.Should()
                .ThrowAsync<DomainException>()
                .WithMessage(DomainErrors.User.NotFound.Message);

            _userRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(default), Times.Never());
        }

        [Fact]
        public async Task ChangePasswordAsync_Should_Return_WhenValidArguments()
        {
            // Arrange
            _userRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(GetUser);

            var userService = new UserService(_dbContextMock.Object, _unitOfWorkMock.Object, _jwtProvider,
                _userRepositoryMock.Object, _passwordHasher);

            // Act
            await userService.ChangePasswordAsync(1, "John@123");

            // Assert
            _userRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(default), Times.Once());
        }

        #endregion

        #region UpdateUserAsync

        [Theory]
        [InlineData("", "")]
        [InlineData(" ", " ")]
        [InlineData(null, null)]
        public async Task UpdateUserAsync_Should_ThrowArgumentException_WhenInvalidNameOrSurname(string name, string surname)
        {
            // Arrange
            _userRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(GetUser);

            var userService = new UserService(_dbContextMock.Object, _unitOfWorkMock.Object, _jwtProvider,
                _userRepositoryMock.Object, _passwordHasher);

            // Act
            var action = () => userService.UpdateUserAsync(1, name, surname);

            // Assert
            await action.Should()
                .ThrowAsync<DomainException>();

            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(default), Times.Never());
        }

        [Fact]
        public async Task UpdateUserAsync_Should_ThrowDomainException_WhenInvalidIdUser()
        {
            // Arrange
            _userRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((User)null);

            var userService = new UserService(_dbContextMock.Object, _unitOfWorkMock.Object, _jwtProvider,
                _userRepositoryMock.Object, _passwordHasher);

            // Act
            var action = () => userService.UpdateUserAsync(1, "Jane", "Doe");

            // Assert
            await action.Should()
                .ThrowAsync<DomainException>()
                .WithMessage(DomainErrors.User.NotFound.Message);

            _userRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(default), Times.Never());
        }

        [Fact]
        public async Task UpdateUserAsync_Should_Return_WhenValidArguments()
        {
            // Arrange
            _userRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(GetUser);

            var userService = new UserService(_dbContextMock.Object, _unitOfWorkMock.Object, _jwtProvider,
                _userRepositoryMock.Object, _passwordHasher);

            // Act
            await userService.UpdateUserAsync(1, "Jane", "Doe");

            // Assert
            _userRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(default), Times.Once());
        }

        #endregion

        #region GetUserByIdAsync

        [Fact]
        public async Task GetUserByIdAsync_Should_ReturnNullUserResponse_WhenInvalidIdUser()
        {
            // Arrange
            _dbContextMock.Setup(x => x.Set<User, int>()).ReturnsDbSet(UserList());
            _dbContextMock.Setup(x => x.Set<Role, byte>()).ReturnsDbSet(RoleList());

            var userService = new UserService(_dbContextMock.Object, _unitOfWorkMock.Object, _jwtProvider,
                _userRepositoryMock.Object, _passwordHasher);

            // Act
            var testResult = await userService.GetUserByIdAsync(default);

            // Arrange
            testResult.Should().BeNull();
        }

        [Fact]
        public async Task GetUserByIdAsync_Should_ReturnDetailedUserResponse_WhenValidIdUser()
        {
            // Arrange
            _dbContextMock.Setup(x => x.Set<User, int>()).ReturnsDbSet(UserList());
            _dbContextMock.Setup(x => x.Set<Role, byte>()).ReturnsDbSet(RoleList());

            var expectedUser = UserList().LastOrDefault();

            var userService = new UserService(_dbContextMock.Object, _unitOfWorkMock.Object, _jwtProvider,
                _userRepositoryMock.Object, _passwordHasher);

            // Act
            var testResult = await userService.GetUserByIdAsync(expectedUser.Id);

            // Arrange
            testResult.Should().NotBeNull();
            testResult.IdUser.Should().Be(expectedUser.Id);
            testResult.Name.Should().Be(expectedUser.Name);
            testResult.Surname.Should().Be(expectedUser.Surname);
            testResult.Email.Should().Be(expectedUser.Email);
            testResult.Role.Should().NotBeNull();
            testResult.Role.IdRole.Should().Be(expectedUser.IdRole);
            testResult.Role.Name.Should().Be(RoleList().FirstOrDefault(x => x.Id == expectedUser.IdRole).Name);
            testResult.CreatedAt.Date.Should().Be(expectedUser.CreatedAt.Date);
        }

        #endregion

        #region GetUsersAsync

        [Fact]
        public async Task GetUsersAsync_Should_ReturnUserResponsePagedList_WhenValidParameters()
        {
            // Arrange
            _dbContextMock.Setup(x => x.Set<User, int>()).ReturnsDbSet(UserList());
            _dbContextMock.Setup(x => x.Set<Role, byte>()).ReturnsDbSet(RoleList());

            var userService = new UserService(_dbContextMock.Object, _unitOfWorkMock.Object, _jwtProvider,
                _userRepositoryMock.Object, _passwordHasher);

            // Act
            var testResult = await userService.GetUsersAsync(new GetUsersRequest(page: 1, pageSize: 10));

            // Arrange
            testResult.Should().NotBeNull();
            testResult.Page.Should().Be(1);
            testResult.PageSize.Should().Be(10);
            testResult.Items.IsNullOrEmpty().Should().BeFalse();
            testResult.TotalCount.Should().Be(UserList().Count());
        }

        [Fact]
        public async Task GetUsersAsync_Should_ReturnEmptyUserResponsePagedList_WhenNotFoundData()
        {
            // Arrange
            _dbContextMock.Setup(x => x.Set<User, int>()).ReturnsDbSet(Enumerable.Empty<User>());
            _dbContextMock.Setup(x => x.Set<Role, byte>()).ReturnsDbSet(Enumerable.Empty<Role>());

            var userService = new UserService(_dbContextMock.Object, _unitOfWorkMock.Object, _jwtProvider,
                _userRepositoryMock.Object, _passwordHasher);

            // Act
            var testResult = await userService.GetUsersAsync(new GetUsersRequest(page: 1, pageSize: 10));

            // Arrange
            testResult.Should().NotBeNull();
            testResult.Page.Should().Be(1);
            testResult.PageSize.Should().Be(10);
            testResult.Items.IsNullOrEmpty().Should().BeTrue();
            testResult.TotalCount.Should().Be(testResult.Items.Count);
        }

        #endregion

        #endregion

        #region Private Methods

        private UserTest GetUser() => new UserTest
        (
            idUser: 1,
            name: "John",
            surname: "Doe",
            email: Email.Create("john.doe@test.com"),
            userRole: UserRoles.General,
            passwordHash: _passwordHasher.HashPassword(Password.Create("John@123"))
        );

        private IOptions<JwtSettings> JwtOptions => Options.Create<JwtSettings>(new JwtSettings
        {
            Issuer = "http://localhost",
            Audience = "http://localhost",
            SecurityKey = "f143bfc760543ec317abd4e8748d9f2b44dfb07a",
            TokenExpirationInMinutes = 60
        });

        private IEnumerable<UserTest> UserList()
        {
            yield return new UserTest(1, "Root", "App", Email.Create("root@test.com"), UserRoles.Administrator, _passwordHasher.HashPassword(Password.Create("Root@123")));
            yield return new UserTest(2, "John", "Doe", Email.Create("john@test.com"), UserRoles.General, _passwordHasher.HashPassword(Password.Create("John@123")));
            yield return new UserTest(3, "Jane", "Doe", Email.Create("jane@test.com"), UserRoles.Analyst, _passwordHasher.HashPassword(Password.Create("Jane@123")));
        }

        private IEnumerable<Role> RoleList()
        {
            yield return new Role(idRole: (byte)UserRoles.Administrator, name: "Administrador");
            yield return new Role(idRole: (byte)UserRoles.General, name: "Geral");
            yield return new Role(idRole: (byte)UserRoles.Analyst, name: "Analista");
        }

        #endregion
    }
}
