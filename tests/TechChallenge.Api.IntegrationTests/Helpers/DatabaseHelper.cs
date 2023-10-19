using System.Data.Common;
using System.Threading.Tasks;

namespace TechChallenge.Api.IntegrationTests.Helpers
{
    internal static class DatabaseHelper
    {
        #region Public Methods

        public static async Task ResetUsersTableAsync(DbConnection connection)
            => await ResetTableAsync(connection, tableName: "dbo.users", reseedValue: 10_005);

        #endregion

        #region Private Methods

        private static async Task ResetTableAsync(DbConnection connection, string tableName, int? reseedValue = null, int? commandTimeout = null)
        {
            await using var tx = await connection.BeginTransactionAsync();
            await using var cmd = connection.CreateCommand();

            cmd.CommandTimeout = commandTimeout ?? cmd.CommandTimeout;
            cmd.CommandText = string.Concat($"DELETE FROM {tableName} ", reseedValue.HasValue ? $"WHERE Id > {reseedValue};" : ";");
            cmd.Transaction = tx;

            await cmd.ExecuteNonQueryAsync();

            if (reseedValue.HasValue)
            {
                cmd.CommandText = $"DBCC CHECKIDENT('{tableName}', RESEED, {reseedValue});";
                await cmd.ExecuteNonQueryAsync();
            }

            await tx.CommitAsync();
        }

        #endregion
    }
}
