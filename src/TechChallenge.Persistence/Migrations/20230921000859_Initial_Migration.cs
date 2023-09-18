using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TechChallenge.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Initial_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "priorities",
                columns: table => new
                {
                    Id = table.Column<byte>(type: "tinyint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Sla = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_priorities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    Id = table.Column<byte>(type: "tinyint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ticketstatus",
                columns: table => new
                {
                    Id = table.Column<byte>(type: "tinyint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ticketstatus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    IdPriority = table.Column<byte>(type: "tinyint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_categories_priorities_IdPriority",
                        column: x => x.IdPriority,
                        principalTable: "priorities",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdRole = table.Column<byte>(type: "tinyint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Surname = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_users_roles_IdRole",
                        column: x => x.IdRole,
                        principalTable: "roles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "tickets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdCategory = table.Column<int>(type: "int", nullable: false),
                    IdStatus = table.Column<byte>(type: "tinyint", nullable: false),
                    IdUserRequester = table.Column<int>(type: "int", nullable: false),
                    IdUserAssigned = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "varchar(max)", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancellationReason = table.Column<string>(type: "varchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedBy = table.Column<int>(type: "int", nullable: true),
                    LastUpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tickets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tickets_categories_IdCategory",
                        column: x => x.IdCategory,
                        principalTable: "categories",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tickets_ticketstatus_IdStatus",
                        column: x => x.IdStatus,
                        principalTable: "ticketstatus",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tickets_users_IdUserAssigned",
                        column: x => x.IdUserAssigned,
                        principalTable: "users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tickets_users_IdUserRequester",
                        column: x => x.IdUserRequester,
                        principalTable: "users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tickets_users_LastUpdatedBy",
                        column: x => x.LastUpdatedBy,
                        principalTable: "users",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "priorities",
                columns: new[] { "Id", "CreatedAt", "IsDeleted", "LastUpdatedAt", "Name", "Sla" },
                values: new object[,]
                {
                    { (byte)1, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, null, "Baixa", 48 },
                    { (byte)2, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, null, "Média", 24 },
                    { (byte)3, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, null, "Alta", 8 },
                    { (byte)4, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, null, "Crítico", 4 }
                });

            migrationBuilder.InsertData(
                table: "roles",
                columns: new[] { "Id", "CreatedAt", "Description", "IsDeleted", "LastUpdatedAt", "Name" },
                values: new object[,]
                {
                    { (byte)1, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, false, null, "Administrador" },
                    { (byte)2, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, false, null, "Usuário" },
                    { (byte)3, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, false, null, "Analista" }
                });

            migrationBuilder.InsertData(
                table: "ticketstatus",
                columns: new[] { "Id", "CreatedAt", "Description", "IsDeleted", "LastUpdatedAt", "Name" },
                values: new object[,]
                {
                    { (byte)1, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, false, null, "Novo" },
                    { (byte)2, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, false, null, "Atribuído" },
                    { (byte)3, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, false, null, "Em andamento" },
                    { (byte)4, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, false, null, "Em espera" },
                    { (byte)5, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, false, null, "Concluído" },
                    { (byte)6, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, false, null, "Cancelado" }
                });

            migrationBuilder.InsertData(
                table: "categories",
                columns: new[] { "Id", "CreatedAt", "Description", "IdPriority", "IsDeleted", "LastUpdatedAt", "Name" },
                values: new object[,]
                {
                    { 1, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, (byte)4, false, null, "Indisponibilidade" },
                    { 2, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, (byte)3, false, null, "Lentidão" },
                    { 3, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, (byte)2, false, null, "Requisição" },
                    { 4, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, (byte)1, false, null, "Dúvida" }
                });

            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "Id", "CreatedAt", "IdRole", "IsDeleted", "LastUpdatedAt", "Name", "Surname", "PasswordHash", "Email" },
                values: new object[,]
                {
                    { 10000, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), (byte)1, false, null, "Administrador", "(built-in)", "MUKOsLOjfoh4YY1ZZLlp+CTyODjmgHhvPAp7PxFiCAWgXo1wibTbOrqht1UhnQi1", "admin@techchallenge.app" },
                    { 10001, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), (byte)2, false, null, "Ailton", "(built-in)", "LFhLAgFT8oinF3iXkk63ccZhEllpvGtr/OHG28On+hqniGeX+AIYe8UhNnqztEIm", "ailton@techchallenge.app" },
                    { 10002, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), (byte)3, false, null, "Bruno", "(built-in)", "yobUq3aH9/R2x//xYdfaxqX2+FVBBLKzLipbFZILjsTo2sJ9cU/f2F4q6vvwIRzs", "bruno@techchallenge.app" },
                    { 10003, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), (byte)2, false, null, "Cecília", "(built-in)", "LSHTSlFvEBDMS0tjoK2po682H7rLfgL2sXssgm/djzWWouzW4lIydGie7PbmX/1P", "cecilia@techchallenge.app" },
                    { 10004, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), (byte)3, false, null, "Cesar", "(built-in)", "q1EyG7yB1S6Cwm7DGuDo3P8ZraEvVHTdBbKHZ1QW3TMG5JWVCnb3EO3UslYiiGeL", "cesar@techchallenge.app" },
                    { 10005, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), (byte)2, false, null, "Paulo", "(built-in)", "XAro1VAlABuvkw5sxcSPEUdCeuTZRcM+9qLOumd79674Ro2V0bvvnlgb3zIkA7Yt", "paulo@techchallenge.app" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_categories_IdPriority",
                table: "categories",
                column: "IdPriority");

            migrationBuilder.CreateIndex(
                name: "IX_tickets_IdCategory",
                table: "tickets",
                column: "IdCategory");

            migrationBuilder.CreateIndex(
                name: "IX_tickets_IdStatus",
                table: "tickets",
                column: "IdStatus");

            migrationBuilder.CreateIndex(
                name: "IX_tickets_IdUserAssigned",
                table: "tickets",
                column: "IdUserAssigned");

            migrationBuilder.CreateIndex(
                name: "IX_tickets_IdUserRequester",
                table: "tickets",
                column: "IdUserRequester");

            migrationBuilder.CreateIndex(
                name: "IX_tickets_LastUpdatedBy",
                table: "tickets",
                column: "LastUpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_users_IdRole",
                table: "users",
                column: "IdRole");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tickets");

            migrationBuilder.DropTable(
                name: "categories");

            migrationBuilder.DropTable(
                name: "ticketstatus");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "priorities");

            migrationBuilder.DropTable(
                name: "roles");
        }
    }
}
