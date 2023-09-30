using System.Threading.Tasks;
using TechChallenge.Domain.Entities;


namespace TechChallenge.Domain.Repositories
{
    public interface ITicketRepository
    {
        #region ITicketRepository Members

        void AssignToUserAsync(int idTicket, int idAssignedUser);

        void CancelTicketAsync(int idTicket, string cancellationReason);

        void CreateTicketAsync(string description, int idCategory);

        Task<Ticket> GetTicketByIdAsync(int idTicket);

        Task<Ticket> GetTicketAsync();

        Task<Ticket> GetByIdAsync(int idTicket);

        void UpdateTicketAsync(int idTicket, int idCategory, string description);

        void UpdateTicketStatusAsync(int idTicket, int ticketStatus);

        void Insert(Ticket ticket);

        #endregion

    }
}
