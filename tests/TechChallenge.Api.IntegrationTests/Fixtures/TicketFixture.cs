using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Extensions;
using TechChallenge.Domain.Enumerations;
using TechChallenge.Api.IntegrationTests.SeedWork;

namespace TechChallenge.Api.IntegrationTests.Fixtures
{
    internal sealed class TicketFixture
    {
        #region Read-Only Fields

        private readonly UserFixture _userFixture;
        private readonly TestHostFixture _testHostFixture;
        private readonly CategoryFixture _categoryFixture;

        #endregion

        #region Constructors

        public TicketFixture(TestHostFixture testHostFixture)
        {
            _testHostFixture = testHostFixture;
            _userFixture = new UserFixture(testHostFixture);
            _categoryFixture = new CategoryFixture(testHostFixture);
        }

        #endregion

        #region Fixture Methods

        public async Task<(IEnumerable<Ticket> Tickets, IEnumerable<User> Users)> SetFixtureAsync()
        {
            var userList = await _userFixture.SetFixtureAsync();
            var category = (await _categoryFixture.SetFixtureAsync()).Categories.FirstOrDefault();

            var ticketList = new List<Ticket>();

            foreach (var (user, index) in userList.WithIndex(x => x.IdRole == (byte)UserRoles.General))
            {
                var analyst = index % 2 == 0
                    ? userList.FirstOrDefault(x => x.IdRole == (byte)UserRoles.Analyst)
                    : userList.LastOrDefault(x => x.IdRole == (byte)UserRoles.Analyst);

                var newTicket = new Ticket(category, description: "Lorem ipsum dolor sit amet.", userRequester: user);
                ticketList.Add(newTicket);

                var assignedTicket = new Ticket(category, description: "Lorem ipsum dolor sit amet.", userRequester: user);
                assignedTicket.AssignTo(analyst, analyst);
                ticketList.Add(assignedTicket);

                var completedTicket = new Ticket(category, description: "Lorem ipsum dolor sit amet.", userRequester: user);
                completedTicket.AssignTo(analyst, analyst);
                completedTicket.Complete(analyst);
                ticketList.Add(completedTicket);

                var cancelledTicket = new Ticket(category, description: "Lorem ipsum dolor sit amet.", userRequester: user);
                cancelledTicket.AssignTo(analyst, analyst);
                cancelledTicket.Cancel("Purposes of integration tests", analyst);
                ticketList.Add(cancelledTicket);
            }

            await _testHostFixture.ExecuteDbContextAsync(async dbContext =>
            {
                await dbContext.Set<Ticket>().AddRangeAsync(ticketList);
                await dbContext.SaveChangesAsync();
            });

            return (ticketList, userList);
        }

        #endregion
    }
}
