using System;
using TechChallenge.Application.Contracts.Category;

namespace TechChallenge.Application.Contracts.Tickets
{
    /// <summary>
    /// Represents the detailed ticket response.
    /// </summary>
    public sealed class DetailedTicketResponse
    {
        /// <summary>
        /// Gets or sets the ticket identifier.
        /// </summary>
        public int IdTicked { get; set; }

        /// <summary>
        /// Gets or sets the ticket description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the ticket category.
        /// </summary>
        public CategoryReponse Category { get; set; }

        /// <summary>
        /// Gets or sets the ticket status.
        /// </summary>
        public StatusResponse Status { get; set; }

        /// <summary>
        /// Gets or sets the ticket created date.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the ticket last updated date.
        /// </summary>
        public DateTime? LastUpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets the ticket last updated user.
        /// </summary>
        public int? LastUpdatedBy { get; set; }

        /// <summary>
        /// Gets or sets the ticket cancellation reason.
        /// </summary>
        public string CancellationReason { get; set; }

        /// <summary>
        /// Gets or sets the ticket id user requester.
        /// </summary>
        public int IdUserRequester { get; set; }

        /// <summary>
        /// Gets or sets the ticket id user assigned.
        /// </summary>
        public int? IdUserAssigned { get; set; }
    }
}
