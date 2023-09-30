using System.Threading.Tasks;
using TechChallenge.Application.Core.Abstractions.Data;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Repositories;
using TechChallenge.Domain.ValueObjects;
using TechChallenge.Persistence.Core.Primitives;

namespace TechChallenge.Persistence.Repositories
{
    internal sealed class TicketRepository : GenericRepository<Ticket, int>, ITicketRepository
    {
        #region Constructors

        public TicketRepository(IDbContext dbContext)
            : base(dbContext)
        { }

        #endregion

        #region ITicketRepository Members


        public void AssignToUserAsync(int idTicket, int idAssignedUser)
        {
            throw new System.NotImplementedException();
        }

        public void CancelTicketAsync(int idTicket, string cancellationReason)
        {
            throw new System.NotImplementedException();
        }

        public void CreateTicketAsync(string description, int idCategory)
        {
            throw new System.NotImplementedException();
        }

        public Task<Ticket> GetTicketAsync()
        {
            throw new System.NotImplementedException();
        }

        public Task<Ticket> GetTicketByIdAsync(int idTicket)
        {
            throw new System.NotImplementedException();
        }

        public void UpdateTicketAsync(int idTicket, int idCategory, string description)
        {
            throw new System.NotImplementedException();
        }

        public void UpdateTicketStatusAsync(int idTicket, int ticketStatus)
        {
            throw new System.NotImplementedException();
        }


        #endregion
    }
}
