using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TechChallenge.Domain.Entities;
using TechChallenge.Api.IntegrationTests.SeedWork;

namespace TechChallenge.Api.IntegrationTests.Fixtures
{
    internal sealed class TicketStatusFixture
    {
        #region Read-Only Fields

        private readonly TestHostFixture _testHostFixture;

        #endregion

        #region Constructors

        public TicketStatusFixture(TestHostFixture testHostFixture)
        {
            _testHostFixture = testHostFixture;
        }

        #endregion

        #region Fixture Methods

        public async Task<IEnumerable<TicketStatus>> SetFixtureAsync()
        {
            var ticketStatus = new List<TicketStatus>();            

            await _testHostFixture.ExecuteDbContextAsync(async dbContext =>
            {
                ticketStatus = await dbContext.Set<TicketStatus>()
                    .AsNoTracking()
                    .ToListAsync();
            });

            return ticketStatus;
        }

        #endregion
    }
}
