namespace TechChallenge.Application.Contracts.Tickets
{
    /// <summary>
    /// Represents the request to assign the ticket to other users.
    /// </summary>
    public sealed class AssignToRequest
    {
        #region Properties

        /// <summary>
        /// Gets or sets the assigned user.
        /// </summary>
        public int IdUserAssigned { get; set; }

        #endregion
    }
}
