namespace TechChallenge.Application.Contracts.Tickets
{
    /// <summary>
    /// Represents the request to change the ticket status.
    /// </summary>
    public sealed class ChangeStatusRequest
    {
        #region Properties

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        public byte IdStatus { get; set; }

        #endregion
    }
}
