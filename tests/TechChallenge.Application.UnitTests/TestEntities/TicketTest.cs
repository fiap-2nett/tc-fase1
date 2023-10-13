using TechChallenge.Domain.Entities;

namespace TechChallenge.Application.UnitTests.TestEntities
{
    internal class TicketTest : Ticket
    {
        public TicketTest(int idTicket, Category category, string description, User userRequester)
            : base(category, description, userRequester)
        {
            Id = idTicket;
        }

        public static TicketTest Create(int idTicket, Category category, string description, User userRequester, User userAssigned = null)
        {
            var ticket = new TicketTest(idTicket, category, description, userRequester);
            if (userAssigned is not null)
                ticket.AssignTo(userAssigned, userAssigned);

            return ticket;
        }
    }
}
