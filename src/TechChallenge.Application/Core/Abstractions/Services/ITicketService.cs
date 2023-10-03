using System.Threading.Tasks;
using TechChallenge.Application.Contracts.Common;
using TechChallenge.Application.Contracts.Tickets;

namespace TechChallenge.Application.Core.Abstractions.Services
{
    public interface ITicketService
    {
        #region ITicketService Members

        Task<DetailedTicketResponse> GetTicketByIdAsync(int idTicket, int idUser);
        Task<PagedList<TicketResponse>> GetTicketsAsync(GetTicketsRequest request, int idUser);
        Task CreateAsync(int idCategory, int idUserRequester, string description);
        Task UpdateAsync(int idTicket, int idCategory, string description);
        Task ChangeStatusAsync(int idTicket, int ticketStatus);
        Task CancelAsync(int idTicket, string cancellationReason);
        Task AssignToUserAsync(int idTicket, int idUserAssigned);

        #endregion
    }
}
