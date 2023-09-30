using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TechChallenge.Api.Contracts;
using TechChallenge.Api.Infrastructure;
using TechChallenge.Application.Contracts.Common;
using TechChallenge.Application.Contracts.Tickets;
using TechChallenge.Application.Core.Abstractions.Authentication;
using TechChallenge.Application.Core.Abstractions.Services;
using TechChallenge.Application.Contracts.Authentication;
using TechChallenge.Domain.Entities;
using TechChallenge.Domain.Enumerations;
using System.Linq;
using System.Reflection.Metadata;
using TechChallenge.Persistence;
using TechChallenge.Application.Contracts.Users;
using static TechChallenge.Domain.Errors.DomainErrors;
using TechChallenge.Domain.Exceptions;
using Microsoft.IdentityModel.Tokens;

namespace TechChallenge.Api.Controllers;

public sealed class TicketsController : ApiController
{

    #region Read-Only Fields

    private readonly ITicketService _ticketService;
    private readonly IUserSessionProvider _userSessionProvider;

    #endregion

    #region Constructors

    public TicketsController(ITicketService ticketService, IUserSessionProvider userSessionProvider)
    {
        _ticketService = ticketService ?? throw new ArgumentNullException(nameof(ticketService));
        _userSessionProvider = userSessionProvider ?? throw new ArgumentNullException(nameof(userSessionProvider));

    }

    #endregion

    #region Endpoints

    /// <summary>
    /// Represents the query for creating a new ticket.
    /// </summary>
    /// <param name="page">The page.</param>
    /// <param name="pageSize">The page size. The max page size is 100.</param>
    /// <returns>The paged list of the tickets.</returns>
    [Consumes("application/json")]
    [Produces("application/json")]
    [HttpGet(ApiRoutes.Tickets.GetAllTickets)]
    [ProducesResponseType(typeof(PagedList<TicketResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAllTickets(int page, int pageSize)
    {
        var response = await _ticketService.GetTicketAsync(new GetTicketsRequest(page, pageSize), _userSessionProvider.IdUser);
        return Ok(response);
    }

    /// <summary>
    /// Represents the query for getting a specific ticket.
    /// </summary>
    /// <returns>The detailed  ticket info.</returns>
    [Consumes("application/json")]
    [Produces("application/json")]
    
    [HttpGet(ApiRoutes.Tickets.GetByIdTickets)]
    [ProducesResponseType(typeof(DetailedTicketResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDetailed(int idTicket)
    {
        var response = await _ticketService.GetTicketByIdAsync(idTicket);
        return Ok(response);
    }

    [HttpPost(ApiRoutes.Tickets.CreateTicket)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] TicketRequest ticketRequest)
    {

        var response = await _ticketService.CreateTicketAsync(ticketRequest.IdCompany, ticketRequest.IdCategory, _userSessionProvider.IdUser, ticketRequest.Description);
        return Ok(response);
    }


    /// <summary>
    /// Represents the query for assigne a ticket.
    /// </summary>
    /// <param idTicket="idTicket">The ticket id to assigne.</param>
    [HttpPost(ApiRoutes.Tickets.AssigneTicket)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AssigneUser(int idTicket)
    {

        var response = await _ticketService.AssignToUserAsync(idTicket, _userSessionProvider.IdUser);

        return Ok(response);
    }


    #endregion


}
