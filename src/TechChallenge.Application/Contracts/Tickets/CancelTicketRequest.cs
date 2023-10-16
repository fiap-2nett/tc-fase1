namespace TechChallenge.Application.Contracts.Tickets
{
    /// <summary>
    /// Represents the cancel ticket request.
    /// </summary>
    public sealed class CancelTicketRequest
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
