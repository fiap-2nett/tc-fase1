namespace TechChallenge.Application.Contracts.Users
{
    /// <summary>
    /// Represents the update user request.
    /// </summary>
    public sealed class UpdateUserRequest
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the surname.
        /// </summary>
        public string Surname { get; set; }
    }
}
