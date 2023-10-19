using System.Threading.Tasks;
using System.Collections.Generic;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Enumerations;
using TechChallenge.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;
using TechChallenge.Api.IntegrationTests.SeedWork;
using TechChallenge.Application.Core.Abstractions.Cryptography;

namespace TechChallenge.Api.IntegrationTests.Fixtures
{
    internal sealed class UserFixture
    {
        #region Read-Only Fields

        private readonly TestHostFixture _testHostFixture;
        private readonly IPasswordHasher _passwordHasher;

        #endregion

        #region Constructors

        public UserFixture(TestHostFixture testHostFixture)
        {
            _testHostFixture = testHostFixture;
            _passwordHasher  = testHostFixture.Server.Host.Services.GetRequiredService<IPasswordHasher>();
        }

        #endregion

        #region Fixture Methods

        public async Task<IEnumerable<User>> SetFixtureAsync()
        {
            var userList = new List<User>()
            {
                new User(name: "Admin", surname: "Test", email: Email.Create("admin.test@test.com"), userRole: UserRoles.Administrator,
                    passwordHash: _passwordHasher.HashPassword(Password.Create("AdminTest@123"))),
                new User(name: "User1", surname: "Test", email: Email.Create("user1.test@test.com"), userRole: UserRoles.General,
                    passwordHash: _passwordHasher.HashPassword(Password.Create("User1Test@123"))),
                new User(name: "User2", surname: "Test", email: Email.Create("user2.test@test.com"), userRole: UserRoles.General,
                    passwordHash: _passwordHasher.HashPassword(Password.Create("User2Test@123"))),
                new User(name: "Tech1", surname: "Test", email: Email.Create("tech1.test@test.com"), userRole: UserRoles.Analyst,
                    passwordHash: _passwordHasher.HashPassword(Password.Create("Tech1Test@123"))),
                new User(name: "Tech2", surname: "Test", email: Email.Create("tech2.test@test.com"), userRole: UserRoles.Analyst,
                    passwordHash: _passwordHasher.HashPassword(Password.Create("Tech2Test@123"))),
            };

            await _testHostFixture.ExecuteDbContextAsync(async dbContext =>
            {
                await dbContext.Set<User>().AddRangeAsync(userList);
                await dbContext.SaveChangesAsync();
            });

            return userList;
        }

        public async Task<User> SetFixtureAsync(string name, string surname, string email, string password, UserRoles role = UserRoles.Administrator)
        {
            var newUser = new User(
                name,
                surname,
                Email.Create(email),
                role,
                passwordHash: _passwordHasher.HashPassword(Password.Create(password))
            );

            await _testHostFixture.ExecuteDbContextAsync(async dbContext =>
            {
                await dbContext.Set<User>().AddAsync(newUser);
                await dbContext.SaveChangesAsync();
            });

            return newUser;
        }

        #endregion
    }
}
