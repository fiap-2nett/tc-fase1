namespace TechChallenge.Persistence.Infrastructure
{
    public sealed record ConnectionString(string Value)
    {
        #region Constants

        public const string SettingsKey = "TechChallengeDb";

        #endregion

        #region Operators

        public static implicit operator string(ConnectionString connectionString)
            => connectionString.Value;

        #endregion
    }
}
