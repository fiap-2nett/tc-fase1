using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using TechChallenge.Api.Contracts;
using TechChallenge.Api.Infrastructure;
using TechChallenge.Application.Core.Abstractions.Services;

namespace TechChallenge.Api.Controllers
{
    public sealed class StatusController : ApiController
    {
        #region Ready-Only Fields

        private readonly ITicketStatusService _ticketStatusService;

        #endregion

        #region Constructors

        public StatusController(ITicketStatusService ticketStatusService)
        {
            _ticketStatusService = ticketStatusService ?? throw new ArgumentNullException(nameof(ticketStatusService));
        }

        #endregion

        #region Endpoints

        /// <summary>
        /// Represents the query to get the ticket statuses list.
        /// </summary>
        /// <returns>The list of tickets statuses</returns>       
        [HttpGet(ApiRoutes.TicketStatus.Get)]
        [ProducesResponseType(StatusCodes.Status200OK)]        
        public async Task<IActionResult> Get()
            => Ok(await _ticketStatusService.GetAsync());

        /// <summary>
        /// Represents the query to get the ticket status by its id.
        /// </summary>
        /// <param name="idTicketStatus"></param>
        /// <returns>One ticket status</returns>                
        [HttpGet(ApiRoutes.TicketStatus.GetById)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get([FromRoute] byte idTicketStatus)
        {
            var response = await _ticketStatusService.GetByIdAsync(idTicketStatus);
            if (response is null) return NotFound();

            return Ok(response);
        }

        #endregion
    }
}
