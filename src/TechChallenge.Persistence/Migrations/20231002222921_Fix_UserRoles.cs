using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechChallenge.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Fix_UserRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "roles",
                keyColumn: "Id",
                keyValue: (byte)2,
                column: "Name",
                value: "Geral");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "roles",
                keyColumn: "Id",
                keyValue: (byte)2,
                column: "Name",
                value: "Usuário");
        }
    }
}
