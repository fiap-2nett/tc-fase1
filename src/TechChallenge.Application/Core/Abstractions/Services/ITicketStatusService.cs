using System.Collections.Generic;
using System.Threading.Tasks;
using TechChallenge.Application.Contracts.Tickets;

namespace TechChallenge.Application.Core.Abstractions.Services
{
    public interface ITicketStatusService
    {
        #region ITicketStatusService Members

        Task<IEnumerable<StatusResponse>> GetAsync();
        Task<StatusResponse> GetByIdAsync(byte idTicketStatus);

        #endregion
    }
}
