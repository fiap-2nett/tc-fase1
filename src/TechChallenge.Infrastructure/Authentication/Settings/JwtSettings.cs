namespace TechChallenge.Infrastructure.Authentication.Settings
{
    public sealed class JwtSettings
    {
        #region Constants

        public const string SettingsKey = "Jwt";

        #endregion

        #region Properties

        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public string SecurityKey { get; set; } = string.Empty;
        public int TokenExpirationInMinutes { get; set; } = 60;

        #endregion
    }
}
