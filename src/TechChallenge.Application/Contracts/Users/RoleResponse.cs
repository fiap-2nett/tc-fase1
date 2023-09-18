namespace TechChallenge.Application.Contracts.Users
{
    /// <summary>
    /// Represents the role response.
    /// </summary>
    public sealed class RoleResponse
    {
        /// <summary>
        /// Gets or sets the role identifier.
        /// </summary>
        public int IdRole { get; set; }

        /// <summary>
        /// Gets or sets the role name.
        /// </summary>
        public string Name { get; set; }
    }
}
