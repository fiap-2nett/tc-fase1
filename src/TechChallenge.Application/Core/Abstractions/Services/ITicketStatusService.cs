using System.Collections.Generic;
using System.Threading.Tasks;
using TechChallenge.Application.Contracts.Tickets;

namespace TechChallenge.Application.Core.Abstractions.Services
{
    public interface ITicketStatusService
    {
        Task<StatusResponse> GetByIdAsync(int idTicketStatus);
        Task<IEnumerable<StatusResponse>> GetAsync();
    }
}
