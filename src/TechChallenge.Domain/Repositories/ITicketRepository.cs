using System.Threading.Tasks;
using TechChallenge.Domain.Entities;

namespace TechChallenge.Domain.Repositories
{
    public interface ITicketRepository
    {
        #region ITicketRepository Members

        Task<Ticket> GetByIdAsync(int idTicket);
        void Insert(Ticket ticket);

        #endregion
    }
}
