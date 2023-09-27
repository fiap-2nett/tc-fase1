using System;
using System.Threading.Tasks;
using TechChallenge.Application.Contracts.Tickets;
using TechChallenge.Application.Core.Abstractions.Services;

namespace TechChallenge.Application.Services;

public sealed class TicketService : ITicketService
{
    public Task AssignToUserAsync(int idTicket, int idAssignedUser)
    {
        throw new NotImplementedException();
    }

    public Task CancelTicketAsync(int idTicket, string cancellationReason)
    {
        throw new NotImplementedException();
    }

    public Task<string> CreateTicketAsync(string description, int idCategory)
    {
        throw new NotImplementedException();
    }

    public Task<DetailedTicketResponse> GetTicketByIdAsync(int idTicket)
    {
        throw new NotImplementedException();
    }

    public Task UpdateTicketAsync(int idTicket, int idCategory, string description)
    {
        throw new NotImplementedException();
    }

    public Task UpdateTicketStatusAsync(int idTicket, int ticketStatus)
    {
        throw new NotImplementedException();
    }
}
