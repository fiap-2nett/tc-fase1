using System.Threading.Tasks;
using TechChallenge.Application.Contracts.Common;
using TechChallenge.Application.Contracts.Tickets;
using TechChallenge.Domain.Enumerations;

namespace TechChallenge.Application.Core.Abstractions.Services
{
    public interface ITicketService
    {
        #region ITicketService Members

        Task<DetailedTicketResponse> GetTicketByIdAsync(int idTicket, int idUser);
        Task<PagedList<TicketResponse>> GetTicketsAsync(GetTicketsRequest request, int idUser);
        Task CreateAsync(int idCategory, int idUserRequester, string description);
        Task UpdateAsync(int idTicket, int idCategory, string description);
        Task ChangeStatusAsync(int idTicket, TicketStatuses changedStatus, int idUserPerformedAction);
        Task CancelAsync(int idTicket, string cancellationReason);
        Task AssignToUserAsync(int idTicket, int idUserAssigned);
        Task CompleteAsync(int idTicket, int idUserPerformedAction);

        #endregion
    }
}
