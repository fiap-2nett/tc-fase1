namespace TechChallenge.Application.Contracts.Users
{
    /// <summary>
    /// Represents the change password request.
    /// </summary>
    public sealed class ChangePasswordRequest
    {
        /// <summary>
        /// Gets or sets the new password.
        /// </summary>
        public string Password { get; set; }
    }
}
