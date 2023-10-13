using System.Threading.Tasks;
using TechChallenge.Application.Contracts.Common;
using TechChallenge.Application.Contracts.Tickets;
using TechChallenge.Domain.Enumerations;

namespace TechChallenge.Application.Core.Abstractions.Services
{
    public interface ITicketService
    {
        #region ITicketService Members

        Task<DetailedTicketResponse> GetTicketByIdAsync(int idTicket, int idUserPerformedAction);
        Task<PagedList<TicketResponse>> GetTicketsAsync(GetTicketsRequest request, int idUserPerformedAction);
        Task<int> CreateAsync(int idCategory, string description, int idUserRequester);
        Task UpdateAsync(int idTicket, int idCategory, string description, int idUserPerformedAction);
        Task ChangeStatusAsync(int idTicket, TicketStatuses changedStatus, int idUserPerformedAction);
        Task CancelAsync(int idTicket, string cancellationReason);
        Task AssignToUserAsync(int idTicket, int idUserAssigned, int idUserPerformedAction);
        Task CompleteAsync(int idTicket, int idUserPerformedAction);

        #endregion
    }
}
