using System.Threading.Tasks;
using TechChallenge.Application.Contracts.Tickets;

namespace TechChallenge.Application.Core.Abstractions.Services;

public interface ITicketService
{
    #region ITicketService Members

    Task<DetailedTicketResponse> GetTicketByIdAsync(int idTicket);
    //Task<PagedList<TicketResponse>> GetTicketAsync(GetTicketRequest request);
    Task<string> CreateTicketAsync(string description, int idCategory);
    Task UpdateTicketAsync(int idTicket, int idCategory, string description);
    Task UpdateTicketStatusAsync(int idTicket, int ticketStatus);
    Task CancelTicketAsync(int idTicket, string cancellationReason);
    Task AssignToUserAsync(int idTicket, int idAssignedUser);

    #endregion
}
