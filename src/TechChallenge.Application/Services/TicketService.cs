using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TechChallenge.Application.Contracts.Common;
using TechChallenge.Application.Contracts.Tickets;
using TechChallenge.Application.Core.Abstractions.Authentication;
using TechChallenge.Application.Core.Abstractions.Cryptography;
using TechChallenge.Application.Core.Abstractions.Data;
using TechChallenge.Application.Core.Abstractions.Services;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Enumerations;
using TechChallenge.Domain.Exceptions;
using TechChallenge.Domain.Extensions;
using TechChallenge.Domain.Repositories;
using static TechChallenge.Domain.Errors.DomainErrors;


namespace TechChallenge.Application.Services;

internal sealed class TicketService : ITicketService
{

    #region Read-Only Fields

    private readonly IDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtProvider _jwtProvider;
    private readonly ITicketRepository _ticketRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUserRepository _userRepository;

    #endregion

    #region Constructors

    public TicketService(IDbContext dbContext,
        IUnitOfWork unitOfWork,
        IJwtProvider jwtProvider,
        ITicketRepository ticketRepository,
        IPasswordHasher passwordHasher,
        IUserRepository userRepository
    )
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _jwtProvider = jwtProvider ?? throw new ArgumentNullException(nameof(jwtProvider));
        _ticketRepository = ticketRepository ?? throw new ArgumentNullException(nameof(ticketRepository));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));

    }

    #endregion

    #region ITicketService Members


    public IQueryable<DetailedTicketResponse> TicketsQueryDetailed(int idUserRole, int idUser, int idTicket)
    {
       
        
        IQueryable<DetailedTicketResponse>  ticketsQuery = (
                from tickets in _dbContext.Set<Ticket, int>().AsNoTracking()
                where tickets.Id == idTicket
                join status in _dbContext.Set<TicketStatus, byte>().AsNoTracking()
                    on tickets.IdStatus equals status.Id
                where tickets.IdStatus == status.Id
                join category in _dbContext.Set<Category, int>().AsNoTracking()
                    on tickets.IdCategory equals category.Id
                where tickets.IdCategory == category.Id

                select new DetailedTicketResponse
                {
                    IdTicked = tickets.Id,
                    Description = tickets.Description,
                    Status = new StatusResponse { IdStatus = status.Id, Name = status.Name },
                    Category = new CategoryReponse { IdCategory = category.Id, Name = category.Name },
                    IdUserRequester = tickets.IdUserRequester,
                    IdUserAssigned = tickets.IdUserAssigned ?? 0,
                    CreatedAt = tickets.CreatedAt,
                    LastUpdatedAt = tickets.LastUpdatedAt,
                    LastUpdatedBy = tickets.LastUpdatedBy,
                    CancellationReason = tickets.CancellationReason
                }
            );


        if (!ticketsQuery.Any())
        {
            throw new DomainException(TicketError.NotFoundError);
        }

        else if (idUserRole == 2)
        {
            ticketsQuery = from tickets in ticketsQuery
                            where tickets.IdUserRequester == idUser
                            select tickets;
            if (!ticketsQuery.Any())
            {
                throw new DomainException(TicketError.InvalidPermissions);
            }
        
        }

        else if (idUserRole == 3)
        {
            ticketsQuery = from tickets in ticketsQuery
                            where tickets.IdUserRequester == idUser || tickets.IdUserAssigned == idUser
                            select tickets;
            if (!ticketsQuery.Any())
            {
                throw new DomainException(TicketError.InvalidPermissions);
            }

        }

        return ticketsQuery;
    }

    public async Task<DetailedTicketResponse> GetTicketByIdAsync(int idTicket, int idUser)
    {
        var user = await _userRepository.GetByIdAsync(idUser);
        IQueryable<DetailedTicketResponse> ticketsQuery;

        if (user.IdRole == ((int)UserRoles.Administrator))
        {
            ticketsQuery = TicketsQueryDetailed((int)UserRoles.Administrator, user.Id, idTicket);
        }

        else if (user.IdRole == ((int)UserRoles.Analyst))
        {
            ticketsQuery = TicketsQueryDetailed((int)UserRoles.Analyst, user.Id, idTicket);
        }

        else

        {
            ticketsQuery = TicketsQueryDetailed((int)UserRoles.General, user.Id, idTicket);
        }

        if (!ticketsQuery.Any())
            throw new DomainException(TicketError.NotFoundError);

        return await ticketsQuery.SingleOrDefaultAsync();
    }

    public async Task<string> AssignToUserAsync(int idTicket, int idAssignedUser)
    {

        var ticket = await _ticketRepository.GetByIdAsync(idTicket);
        if (ticket is null)
            throw new DomainException(TicketError.NotFoundError);
        
        ticket.AssigneUser(idTicket, idAssignedUser);
        await _unitOfWork.SaveChangesAsync();

        return $"Ticket {ticket.Id} atribuido com sucesso.";
    }

    public async Task CancelTicketAsync(int idTicket, string cancellationReason)
    {
        throw new NotImplementedException();
    }

    public async Task<string> CreateTicketAsync(int idCompany, int idCategory, int idUserRequester, string description)
    {

        if (idCategory.ToString().IsNullOrEmpty() || idCategory <= 0 || description.IsNullOrEmpty())
        {
            throw new DomainException(TicketError.InvalidFields);
        }

        var ticket = new Ticket(idCompany, idCategory, idUserRequester, TicketStatuses.New, description);
        

        _ticketRepository.Insert(ticket);
        await _unitOfWork.SaveChangesAsync();

        return $"Ticket {ticket.Id} criado com sucesso.";
    }

    public IQueryable<TicketResponse> TicketsQueryAll(int idUserRole, int idUser)
    {
        IQueryable<TicketResponse> ticketsQuery = (
                from tickets in _dbContext.Set<Ticket, int>().AsNoTracking()
                join status in _dbContext.Set<TicketStatus, byte>().AsNoTracking()
                    on tickets.IdStatus equals status.Id
                where tickets.IdStatus == status.Id
                join category in _dbContext.Set<Category, int>().AsNoTracking()
                    on tickets.IdCategory equals category.Id
                where tickets.IdCategory == category.Id

                select new TicketResponse
                {
                    IdTicked = tickets.Id,
                    Description = tickets.Description,
                    Status = new StatusResponse { IdStatus = status.Id, Name = status.Name },
                    Category = new CategoryReponse { IdCategory = category.Id, Name = category.Name },
                    IdUserRequester = tickets.IdUserRequester,
                    IdUserAssigned = tickets.IdUserAssigned ?? 0

                }
            ) ;

        if ( idUserRole == 2 )
        {
            
            ticketsQuery = from tickets in ticketsQuery
                           where tickets.IdUserRequester == idUser
                           select tickets;
        }

        else if ( idUserRole == 3 )
        {
            ticketsQuery = from tickets in ticketsQuery
                           where tickets.IdUserRequester == idUser || tickets.IdUserAssigned == idUser
                            
                           select tickets;
        }


        return ticketsQuery;
    }

    public async Task<PagedList<TicketResponse>> GetTicketAsync(GetTicketsRequest request, int idUser)
    {
        
        var user = await _userRepository.GetByIdAsync(idUser);
        IQueryable<TicketResponse> ticketsQuery;


        if (user.IdRole == ((int)UserRoles.Administrator))
        {
            ticketsQuery = TicketsQueryAll((int)UserRoles.Administrator, user.Id);
        }

        else if (user.IdRole == ((int)UserRoles.Analyst))
        {
            ticketsQuery = TicketsQueryAll((int)UserRoles.Analyst, user.Id);
        }

        else

        {
            ticketsQuery = TicketsQueryAll((int)UserRoles.General, user.Id);
        }


        var totalCount = await ticketsQuery.CountAsync();

        var ticketsReponsePage = await ticketsQuery
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToArrayAsync();

        return new PagedList<TicketResponse>(ticketsReponsePage, request.Page, request.PageSize, totalCount);
    }

    public async Task UpdateTicketAsync(int idTicket, int idCategory, string description)
    {
        throw new NotImplementedException();
    }

    public async Task UpdateTicketStatusAsync(int idTicket, int ticketStatus)
    {
        throw new NotImplementedException();
    }


    #endregion

}
