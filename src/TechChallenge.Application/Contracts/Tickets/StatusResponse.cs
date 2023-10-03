namespace TechChallenge.Application.Contracts.Tickets
{
    /// <summary>
    /// Represents the status response.
    /// </summary>
    public sealed class StatusResponse
    {
        /// <summary>
        /// Gets or sets the status identifier.
        /// </summary>
        public int IdStatus { get; set; }

        /// <summary>
        /// Gets or sets the status name.
        /// </summary>
        public string Name { get; set; }
    }
}
