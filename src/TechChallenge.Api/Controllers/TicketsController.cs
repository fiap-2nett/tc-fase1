using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using TechChallenge.Api.Contracts;
using TechChallenge.Api.Infrastructure;
using TechChallenge.Application.Contracts.Common;
using TechChallenge.Application.Contracts.Tickets;
using TechChallenge.Application.Core.Abstractions.Authentication;
using TechChallenge.Application.Core.Abstractions.Services;

namespace TechChallenge.Api.Controllers
{
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
        [HttpGet(ApiRoutes.Tickets.Get)]
        [ProducesResponseType(typeof(PagedList<TicketResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(int page, int pageSize)
            => Ok(await _ticketService.GetTicketsAsync(new GetTicketsRequest(page, pageSize), _userSessionProvider.IdUser));

        /// <summary>
        /// Represents the query for getting a specific ticket.
        /// </summary>
        /// <returns>The detailed ticket info.</returns>       
        [HttpGet(ApiRoutes.Tickets.GetById)]
        [ProducesResponseType(typeof(DetailedTicketResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById([FromRoute] int idTicket)
        {
            var response = await _ticketService.GetTicketByIdAsync(idTicket, _userSessionProvider.IdUser);
            return Ok(response);
        }

        /// <summary>
        /// Represents the request to create a ticket.
        /// </summary>
        /// <param name="ticketRequest">Represents the request to create a ticket</param>
        /// <returns></returns>        
        [HttpPost(ApiRoutes.Tickets.Create)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] TicketRequest ticketRequest)
        {
            await _ticketService.CreateAsync(ticketRequest.IdCategory, _userSessionProvider.IdUser, ticketRequest.Description);
            return Ok();
        }

        /// <summary>
        /// Represents the request to assign the ticket to the logged in user.
        /// </summary>
        /// <param idTicket="idTicket">The ticket identifier.</param>
        [HttpPost(ApiRoutes.Tickets.AssignToMe)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AssignToMe([FromRoute] int idTicket)
        {
            await _ticketService.AssignToUserAsync(idTicket, _userSessionProvider.IdUser);
            return Ok();
        }

        #endregion
    }
}
