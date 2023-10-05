using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using TechChallenge.Domain.Errors;
using Microsoft.EntityFrameworkCore;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Exceptions;
using TechChallenge.Domain.Enumerations;
using TechChallenge.Domain.Repositories;
using TechChallenge.Application.Contracts.Common;
using TechChallenge.Application.Contracts.Tickets;
using TechChallenge.Application.Contracts.Category;
using TechChallenge.Application.Core.Abstractions.Data;
using TechChallenge.Application.Core.Abstractions.Services;

namespace TechChallenge.Application.Services
{
    internal sealed class TicketService : ITicketService
    {
        #region Read-Only Fields

        private readonly IDbContext _dbContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserRepository _userRepository;
        private readonly ITicketRepository _ticketRepository;

        #endregion

        #region Constructors

        public TicketService(IDbContext dbContext, IUnitOfWork unitOfWork, ITicketRepository ticketRepository, IUserRepository userRepository) 
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));            
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _ticketRepository = ticketRepository ?? throw new ArgumentNullException(nameof(ticketRepository));            
        }

        #endregion

        #region ITicketService Members

        public async Task<DetailedTicketResponse> GetTicketByIdAsync(int idTicket, int idUser)
        {
            var user = await _userRepository.GetByIdAsync(idUser);

            var ticketResult = await (
                from ticket in _dbContext.Set<Domain.Entities.Ticket, int>().AsNoTracking()
                join status in _dbContext.Set<TicketStatus, byte>().AsNoTracking()
                    on ticket.IdStatus equals status.Id                
                join category in _dbContext.Set<Category, int>().AsNoTracking()
                    on ticket.IdCategory equals category.Id                
                where
                    ticket.Id == idTicket
                select new DetailedTicketResponse
                {
                    IdTicked = ticket.Id,
                    Description = ticket.Description,
                    Status = new StatusResponse { IdStatus = status.Id, Name = status.Name },
                    Category = new CategoryReponse { IdCategory = category.Id, Name = category.Name },
                    IdUserRequester = ticket.IdUserRequester,
                    IdUserAssigned = ticket.IdUserAssigned,
                    CreatedAt = ticket.CreatedAt,
                    LastUpdatedAt = ticket.LastUpdatedAt,
                    LastUpdatedBy = ticket.LastUpdatedBy,
                    CancellationReason = ticket.CancellationReason
                }
            ).FirstOrDefaultAsync();

            if (ticketResult is null)
                throw new NotFoundException(DomainErrors.Ticket.NotFound);
            
            if (user.IdRole == (byte)UserRoles.General && ticketResult.IdUserRequester != user.Id)
                throw new InvalidPermissionException(DomainErrors.User.InvalidPermissions);

            if (user.IdRole == (byte)UserRoles.Analyst && (ticketResult.IdUserRequester != user.Id && ticketResult.IdUserAssigned != user.Id))
                throw new InvalidPermissionException(DomainErrors.User.InvalidPermissions);
            
            return ticketResult;
        }

        public async Task<PagedList<TicketResponse>> GetTicketsAsync(GetTicketsRequest request, int idUser)
        {
            var user = await _userRepository.GetByIdAsync(idUser);

            IQueryable<TicketResponse> ticketsQuery = (
                from ticket in _dbContext.Set<Domain.Entities.Ticket, int>().AsNoTracking()
                join status in _dbContext.Set<TicketStatus, byte>().AsNoTracking()
                    on ticket.IdStatus equals status.Id                    
                join category in _dbContext.Set<Category, int>().AsNoTracking()
                    on ticket.IdCategory equals category.Id                    
                select new TicketResponse
                {
                    IdTicked = ticket.Id,
                    Description = ticket.Description,
                    Status = new StatusResponse { IdStatus = status.Id, Name = status.Name },
                    Category = new CategoryReponse { IdCategory = category.Id, Name = category.Name },
                    IdUserRequester = ticket.IdUserRequester,
                    IdUserAssigned = ticket.IdUserAssigned
                }
            );

            if (user.IdRole == (byte)UserRoles.General)
                ticketsQuery = ticketsQuery.Where(t => t.IdUserRequester == user.Id);

            if (user.IdRole == (byte)UserRoles.Analyst)
                ticketsQuery = ticketsQuery.Where(t => t.IdUserRequester == user.Id || t.IdUserAssigned == user.Id);

            var totalCount = await ticketsQuery.CountAsync();

            var ticketsReponsePage = await ticketsQuery
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToArrayAsync();

            return new PagedList<TicketResponse>(ticketsReponsePage, request.Page, request.PageSize, totalCount);
        }

        public async Task CreateAsync(int idCategory, int idUserRequester, string description)
        {
            var ticket = new Ticket(idCategory, idUserRequester, description);

            _ticketRepository.Insert(ticket);
            await _unitOfWork.SaveChangesAsync();            
        }

        public async Task UpdateAsync(int idTicket, int idCategory, string description)
        {
            throw new NotImplementedException();
        }

        public async Task ChangeStatusAsync(int idTicket, TicketStatuses changedStatus, int idUserPerformedAction)
        {
            var userPerformedAction = await _userRepository.GetByIdAsync(idUserPerformedAction);
            if (userPerformedAction is null)
                throw new NotFoundException(DomainErrors.User.NotFound);

            var ticket = await _ticketRepository.GetByIdAsync(idTicket);
            if (ticket is null)
                throw new NotFoundException(DomainErrors.Ticket.NotFound);
                        
            ticket.ChangeStatus(changedStatus, userPerformedAction);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task CancelAsync(int idTicket, string cancellationReason)
        {
            throw new NotImplementedException();
        }

        public async Task AssignToUserAsync(int idTicket, int idUserAssigned, int idUserPerformedAction)
        {
            var userAssigned = await _userRepository.GetByIdAsync(idUserAssigned);
            if (userAssigned is null)
                throw new NotFoundException(DomainErrors.User.NotFound);

            var userPerformedAction = await _userRepository.GetByIdAsync(idUserPerformedAction);
            if (userPerformedAction is null)
                throw new NotFoundException(DomainErrors.User.NotFound);

            var ticket = await _ticketRepository.GetByIdAsync(idTicket);
            if (ticket is null)
                throw new NotFoundException(DomainErrors.Ticket.NotFound);

            ticket.AssignTo(userAssigned, userPerformedAction);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task CompleteAsync(int idTicket, int idUserPerformedAction)
        {
            var userPerformedAction = await _userRepository.GetByIdAsync(idUserPerformedAction);
            if (userPerformedAction is null)
                throw new NotFoundException(DomainErrors.User.NotFound);

            var ticket = await _ticketRepository.GetByIdAsync(idTicket);
            if (ticket is null)
                throw new NotFoundException(DomainErrors.Ticket.NotFound);

            ticket.Complete(userPerformedAction);
            await _unitOfWork.SaveChangesAsync();
        }

        #endregion
    }
}
