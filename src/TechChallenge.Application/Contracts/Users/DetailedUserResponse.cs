using System;

namespace TechChallenge.Application.Contracts.Users
{
    public sealed class DetailedUserResponse
    {
        /// <summary>
        /// Gets or sets the user identifier.
        /// </summary>
        public int IdUser { get; set; }

        /// <summary>
        /// Gets or sets the user name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the user surname.
        /// </summary>
        public string Surname { get; set; }

        /// <summary>
        /// Gets or sets the user email.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the user role.
        /// </summary>
        public RoleResponse Role { get; set; }

        /// <summary>
        /// Gets the user's creation date.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets the user's last updated date.
        /// </summary>
        public DateTime? LastUpdatedAt { get; set; }
    }
}
