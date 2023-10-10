using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TechChallenge.Application.Contracts.Tickets;
using TechChallenge.Application.Core.Abstractions.Data;
using TechChallenge.Application.Core.Abstractions.Services;
using TechChallenge.Domain.Entities;

namespace TechChallenge.Application.Services
{
    internal sealed class TicketStatusService : ITicketStatusService
    {
        #region Read-Only Fields

        private readonly IDbContext _dbContext;

        #endregion

        #region Constructors

        public TicketStatusService(IDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        #endregion

        #region ITicketStatusService Members

        public async Task<IEnumerable<StatusResponse>> GetAsync()
        {
            IQueryable<StatusResponse> ticketStatusQuery = (
                from ticketStatus in _dbContext.Set<TicketStatus, byte>().AsNoTracking()
                select new StatusResponse
                {
                    IdStatus = ticketStatus.Id,
                    Name = ticketStatus.Name
                }
            );

            return await ticketStatusQuery.ToListAsync();
        }

        public async Task<StatusResponse> GetByIdAsync(int statusId)
        {
            IQueryable<StatusResponse> ticketStatusQuery = (
                from ticketStatus in _dbContext.Set<TicketStatus, byte>().AsNoTracking()
                where
                    ticketStatus.Id == statusId
                select new StatusResponse
                {
                    IdStatus = ticketStatus.Id,
                    Name = ticketStatus.Name,
                }
            );

            return await ticketStatusQuery.FirstOrDefaultAsync();
        }

        #endregion
    }
}
