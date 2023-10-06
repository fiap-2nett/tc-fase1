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
using TechChallenge.Domain.Enumerations;
using TechChallenge.Domain.Exceptions;
using TechChallenge.Domain.Errors;
using TechChallenge.Domain.Helpers;

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
        /// Represents the query to retrieve all tickets.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="pageSize">The page size. The max page size is 100.</param>
        /// <returns>The paged list of the tickets.</returns>
        [HttpGet(ApiRoutes.Tickets.Get)]
        [ProducesResponseType(typeof(PagedList<TicketResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(int page, int pageSize)
            => Ok(await _ticketService.GetTicketsAsync(new GetTicketsRequest(page, pageSize), _userSessionProvider.IdUser));

        /// <summary>
        /// Represents the query to retrieve a specific ticket.
        /// </summary>
        /// <param name="idTicket">The ticket identifier.</param>
        /// <returns>The detailed ticket info.</returns>
        [HttpGet(ApiRoutes.Tickets.GetById)]
        [ProducesResponseType(typeof(DetailedTicketResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById([FromRoute] int idTicket)
            => Ok(await _ticketService.GetTicketByIdAsync(idTicket, _userSessionProvider.IdUser));

        /// <summary>
        /// Represents the request to create a ticket.
        /// </summary>
        /// <param name="createTicketRequest">Represents the request to create a ticket</param>
        /// <returns></returns>
        [HttpPost(ApiRoutes.Tickets.Create)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateTicketRequest createTicketRequest)
        {
            if (createTicketRequest is null)
                throw new DomainException(DomainErrors.Ticket.DataSentIsInvalid);

            var idTicket = await _ticketService.CreateAsync(createTicketRequest.IdCategory,
                createTicketRequest.Description,
                idUserRequester: _userSessionProvider.IdUser);

            return Created(new Uri(Url.ActionLink(nameof(GetById), "Tickets", new { idTicket })), idTicket);
        }

        /// <summary>
        /// Represents the request to assign the ticket to the logged in user.
        /// </summary>
        /// <param name="idTicket">The ticket identifier.</param>
        [HttpPost(ApiRoutes.Tickets.AssignToMe)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AssignToMe([FromRoute] int idTicket)
        {
            await _ticketService.AssignToUserAsync(idTicket,
                idUserAssigned: _userSessionProvider.IdUser,
                idUserPerformedAction: _userSessionProvider.IdUser);

            return Ok();
        }

        /// <summary>
        /// Represents the request to update the ticket.
        /// </summary>
        /// <param name="idTicket">The ticket identifier.</param>
        /// <param name="updateTicketRequest">Represents the request to update the ticket.</param>
        [HttpPut(ApiRoutes.Tickets.Update)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update([FromRoute] int idTicket, [FromBody] UpdateTicketRequest updateTicketRequest)
        {
            if (updateTicketRequest is null)
                throw new DomainException(DomainErrors.Ticket.DataSentIsInvalid);

            await _ticketService.UpdateAsync(idTicket,
                updateTicketRequest.IdCategory,
                updateTicketRequest.Description,
                idUserPerformedAction: _userSessionProvider.IdUser);

            return Ok();
        }

        /// <summary>
        /// Represents the request to assign the ticket to other users.
        /// </summary>
        /// <param name="idTicket">The ticket identifier.</param>
        /// <param name="assignToRequest">Represents the request to assign the ticket to other users</param>
        [HttpPost(ApiRoutes.Tickets.AssignTo)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AssignTo([FromRoute] int idTicket, [FromBody] AssignToRequest assignToRequest)
        {
            if (assignToRequest is null)
                throw new NotFoundException(DomainErrors.User.NotFound);

            await _ticketService.AssignToUserAsync(idTicket,
                idUserAssigned: assignToRequest.IdUserAssigned,
                idUserPerformedAction: _userSessionProvider.IdUser);

            return Ok();
        }

        /// <summary>
        /// Represents the request to complete the ticket.
        /// </summary>
        /// <param name="idTicket">The ticket identifier.</param>
        [HttpPost(ApiRoutes.Tickets.Complete)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Complete([FromRoute] int idTicket)
        {
            await _ticketService.CompleteAsync(idTicket, _userSessionProvider.IdUser);
            return Ok();
        }

        /// <summary>
        /// Represents the request to change the ticket status. Internal Movement only.
        /// </summary>
        /// <param name="idTicket">The ticket identifier.</param>
        /// <param name="changeStatusRequest">Represents the request to change the ticket status.</param>
        [HttpPost(ApiRoutes.Tickets.ChangeStatus)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> ChangeStatus([FromRoute] int idTicket, [FromBody] ChangeStatusRequest changeStatusRequest)
        {
            if (!EnumHelper.TryConvert(changeStatusRequest.IdStatus, out TicketStatuses changedStatus))
                throw new DomainException(DomainErrors.Ticket.StatusDoesNotExist);

            await _ticketService.ChangeStatusAsync(idTicket, changedStatus, _userSessionProvider.IdUser);
            return Ok();
        }

        #endregion
    }
}
