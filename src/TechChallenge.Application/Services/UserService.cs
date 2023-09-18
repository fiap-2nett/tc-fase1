using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TechChallenge.Application.Contracts.Authentication;
using TechChallenge.Application.Contracts.Common;
using TechChallenge.Application.Contracts.Users;
using TechChallenge.Application.Core.Abstractions.Authentication;
using TechChallenge.Application.Core.Abstractions.Cryptography;
using TechChallenge.Application.Core.Abstractions.Data;
using TechChallenge.Application.Core.Abstractions.Services;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Enumerations;
using TechChallenge.Domain.Errors;
using TechChallenge.Domain.Exceptions;
using TechChallenge.Domain.Repositories;
using TechChallenge.Domain.ValueObjects;

namespace TechChallenge.Application.Services
{
    internal sealed class UserService : IUserService
    {
        #region Read-Only Fields

        private readonly IDbContext _dbContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtProvider _jwtProvider;
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;

        #endregion

        #region Constructors

        public UserService(IDbContext dbContext,
            IUnitOfWork unitOfWork,
            IJwtProvider jwtProvider,
            IUserRepository userRepository,
            IPasswordHasher passwordHasher
        )
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _jwtProvider = jwtProvider ?? throw new ArgumentNullException(nameof(jwtProvider));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        }

        #endregion

        #region IUserService Members

        public async Task<TokenResponse> CreateAsync(string name, string surname, string email, string password)
        {
            var emailResult = Email.Create(email);

            if (!await _userRepository.IsEmailUniqueAsync(emailResult))
                throw new DomainException(DomainErrors.User.DuplicateEmail);

            var passwordHash = _passwordHasher.HashPassword(Password.Create(password));
            var user = new User(name, surname, emailResult, UserRoles.User, passwordHash);

            _userRepository.Insert(user);
            await _unitOfWork.SaveChangesAsync();

            return new TokenResponse(_jwtProvider.Create(user));
        }

        public async Task ChangePasswordAsync(int idUser, string password)
        {
            var passwordResult = Password.Create(password);

            var user = await _userRepository.GetByIdAsync(idUser);
            if (user is null)
                throw new DomainException(DomainErrors.User.NotFound);

            var passwordHash = _passwordHasher.HashPassword(passwordResult);

            user.ChangePassword(passwordHash);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateUserAsync(int idUser, string name, string surname)
        {
            var user = await _userRepository.GetByIdAsync(idUser);
            if (user is null)
                throw new DomainException(DomainErrors.User.NotFound);

            user.ChangeName(name, surname);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<DetailedUserResponse> GetUserByIdAsync(int idUser)
        {
            var userQuery = (
                from user in _dbContext.Set<User, int>().AsNoTracking()
                join role in _dbContext.Set<Role, byte>().AsNoTracking()
                    on user.IdRole equals role.Id
                where
                    user.Id == idUser
                select new DetailedUserResponse
                {
                    IdUser = user.Id,
                    Name = user.Name,
                    Surname = user.Surname,
                    Email = user.Email,
                    Role = new RoleResponse { IdRole = role.Id, Name = role.Name },
                    CreatedAt = user.CreatedAt,
                    LastUpdatedAt = user.LastUpdatedAt
                }
            );

            return await userQuery.SingleOrDefaultAsync();
        }

        public async Task<PagedList<UserResponse>> GetUsersAsync(GetUsersRequest request)
        {
            IQueryable<UserResponse> usersQuery = (
                from user in _dbContext.Set<User, int>().AsNoTracking()
                join role in _dbContext.Set<Role, byte>().AsNoTracking()
                    on user.IdRole equals role.Id
                orderby user.Name
                select new UserResponse
                {
                    IdUser = user.Id,
                    FullName = $"{user.Name} {user.Surname}",
                    Role = new RoleResponse { IdRole = role.Id, Name = role.Name  }
                }
            );

            var totalCount = await usersQuery.CountAsync();

            var usersReponsePage = await usersQuery
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToArrayAsync();

            return new PagedList<UserResponse>(usersReponsePage, request.Page, request.PageSize, totalCount);
        }

        #endregion
    }
}
