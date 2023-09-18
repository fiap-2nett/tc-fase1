namespace TechChallenge.Application.Contracts.Users
{
    /// <summary>
    /// Represents the user response.
    /// </summary>
    public sealed class UserResponse
    {
        /// <summary>
        /// Gets or sets the user identifier.
        /// </summary>
        public int IdUser { get; set; }

        /// <summary>
        /// Gets or sets the user fullname.
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Gets or sets the user role.
        /// </summary>
        public RoleResponse Role { get; set; }
    }
}
