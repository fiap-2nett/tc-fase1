using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Enumerations;
using TechChallenge.Domain.ValueObjects;

namespace TechChallenge.Application.UnitTests.TestEntities
{
    internal class UserTest : User
    {
        public UserTest(int idUser, string name, string surname, Email email, UserRoles userRole, string passwordHash)
            : base(name, surname, email, userRole, passwordHash)
        {
            Id = idUser;
        }
    }
}
