using System.Threading.Tasks;
using TechChallenge.Application.Core.Abstractions.Data;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Repositories;
using TechChallenge.Domain.ValueObjects;
using TechChallenge.Persistence.Core.Primitives;

namespace TechChallenge.Persistence.Repositories
{
    internal sealed class UserRepository : GenericRepository<User, int>, IUserRepository
    {
        #region Constructors

        public UserRepository(IDbContext dbContext)
            : base(dbContext)
        { }

        #endregion

        #region IUserRepository Members

        public async Task<User> GetByEmailAsync(Email email)
            => await FirstOrDefaultAsync(user => user.Email.Value == email);

        public async Task<bool> IsEmailUniqueAsync(Email email)
            => !await AnyAsync(user => user.Email.Value == email);

        #endregion
    }
}
