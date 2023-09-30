using System.Threading.Tasks;
using TechChallenge.Application.Contracts.Common;
using TechChallenge.Application.Contracts.Tickets;
using TechChallenge.Domain.Enumerations;

namespace TechChallenge.Application.Core.Abstractions.Services;

public interface ITicketService
{


    #region ITicketService Members

    Task<DetailedTicketResponse> GetTicketByIdAsync(int idTicket);
    Task<PagedList<TicketResponse>> GetTicketAsync(GetTicketsRequest request, int idUser);
    Task<string> CreateTicketAsync(int idCompany, int idCategory, int idUserRequester, string description);
    Task UpdateTicketAsync(int idTicket, int idCategory, string description);
    Task UpdateTicketStatusAsync(int idTicket, int ticketStatus);
    Task CancelTicketAsync(int idTicket, string cancellationReason);
    Task<string> AssignToUserAsync(int idTicket, int idAssignedUser);

    #endregion
}
