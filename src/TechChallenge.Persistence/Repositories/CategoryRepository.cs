using TechChallenge.Application.Core.Abstractions.Data;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Repositories;
using TechChallenge.Persistence.Core.Primitives;

namespace TechChallenge.Persistence.Repositories
{
    internal sealed class CategoryRepository : GenericRepository<Category, int>, ICategoryRepository
    {
        #region Constructors

        public CategoryRepository(IDbContext dbContext)
            : base(dbContext)
        { }

        #endregion
    }
}
