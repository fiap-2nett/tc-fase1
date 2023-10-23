using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TechChallenge.Domain.Entities;
using TechChallenge.Api.IntegrationTests.SeedWork;

namespace TechChallenge.Api.IntegrationTests.Fixtures
{
    internal sealed class CategoryFixture
    {
        #region Read-Only Fields

        private readonly TestHostFixture _testHostFixture;

        #endregion

        #region Constructors

        public CategoryFixture(TestHostFixture testHostFixture)
        {
            _testHostFixture = testHostFixture;
        }

        #endregion

        #region Fixture Methods

        public async Task<(IEnumerable<Category> Categories, IEnumerable<Priority> Priorities)> SetFixtureAsync()
        {
            var categories = new List<Category>();
            var priorities = new List<Priority>();

            await _testHostFixture.ExecuteDbContextAsync(async dbContext =>
            {
                categories = await dbContext.Set<Category>()
                    .AsNoTracking()
                    .ToListAsync();

                priorities = await dbContext.Set<Priority>()
                    .AsNoTracking()
                    .ToListAsync();
            });

            return (categories, priorities);
        }

        #endregion
    }
}
