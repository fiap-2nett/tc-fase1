namespace TechChallenge.Application.Contracts.Tickets
{
    /// <summary>
    /// Represents the request ticket.
    /// </summary>
    public sealed class TicketRequest
    {
        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        public int IdCategory { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string Description { get; set; }
    }
}
