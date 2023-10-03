using TechChallenge.Application.Contracts.Category;

namespace TechChallenge.Application.Contracts.Tickets
{
    /// <summary>
    /// Represents the ticket response.
    /// </summary>
    public sealed class TicketResponse
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
        /// Gets or sets the ticket user requester.
        /// </summary>
        public int IdUserRequester { get; set; }

        /// <summary>
        /// Gets or sets the ticket user assigned.
        /// </summary>
        public int? IdUserAssigned { get; set; }
    }
}
