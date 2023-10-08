namespace TechChallenge.Application.Contracts.Tickets
{
    /// <summary>
    /// Represents the request ticket creation.
    /// </summary>
    public sealed class CreateTicketRequest
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
