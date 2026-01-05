#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class BotRenameandUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserBotProviders");

            migrationBuilder.DropIndex(
                name: "IX_Decks_Name_Id",
                table: "Decks");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "UserBotCodes");

            migrationBuilder.CreateTable(
                name: "UserBots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BotId = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserBots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserBots_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "NoteTypeTemplates",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 10, 6, 30, 611, DateTimeKind.Utc).AddTicks(8397), new DateTime(2026, 1, 3, 10, 6, 30, 611, DateTimeKind.Utc).AddTicks(8398) });

            migrationBuilder.UpdateData(
                table: "NoteTypeTemplates",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 10, 6, 30, 611, DateTimeKind.Utc).AddTicks(9441), new DateTime(2026, 1, 3, 10, 6, 30, 611, DateTimeKind.Utc).AddTicks(9441) });

            migrationBuilder.UpdateData(
                table: "NoteTypeTemplates",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 10, 6, 30, 611, DateTimeKind.Utc).AddTicks(9443), new DateTime(2026, 1, 3, 10, 6, 30, 611, DateTimeKind.Utc).AddTicks(9443) });

            migrationBuilder.UpdateData(
                table: "NoteTypes",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 10, 6, 30, 611, DateTimeKind.Utc).AddTicks(902), new DateTime(2026, 1, 3, 10, 6, 30, 611, DateTimeKind.Utc).AddTicks(904) });

            migrationBuilder.UpdateData(
                table: "NoteTypes",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 10, 6, 30, 611, DateTimeKind.Utc).AddTicks(2032), new DateTime(2026, 1, 3, 10, 6, 30, 611, DateTimeKind.Utc).AddTicks(2033) });

            migrationBuilder.CreateIndex(
                name: "IX_NoteTypes_Name_CreatorId",
                table: "NoteTypes",
                columns: new[] { "Name", "CreatorId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Decks_Name_CreatorId",
                table: "Decks",
                columns: new[] { "Name", "CreatorId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserBots_BotId_UserId",
                table: "UserBots",
                columns: new[] { "BotId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserBots_UserId",
                table: "UserBots",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserBots");

            migrationBuilder.DropIndex(
                name: "IX_NoteTypes_Name_CreatorId",
                table: "NoteTypes");

            migrationBuilder.DropIndex(
                name: "IX_Decks_Name_CreatorId",
                table: "Decks");

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "UserBotCodes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "UserBotProviders",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserBotProviders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserBotProviders_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "NoteTypeTemplates",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 2, 23, 52, 8, 606, DateTimeKind.Utc).AddTicks(9521), new DateTime(2026, 1, 2, 23, 52, 8, 606, DateTimeKind.Utc).AddTicks(9523) });

            migrationBuilder.UpdateData(
                table: "NoteTypeTemplates",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 2, 23, 52, 8, 607, DateTimeKind.Utc).AddTicks(1018), new DateTime(2026, 1, 2, 23, 52, 8, 607, DateTimeKind.Utc).AddTicks(1018) });

            migrationBuilder.UpdateData(
                table: "NoteTypeTemplates",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 2, 23, 52, 8, 607, DateTimeKind.Utc).AddTicks(1020), new DateTime(2026, 1, 2, 23, 52, 8, 607, DateTimeKind.Utc).AddTicks(1020) });

            migrationBuilder.UpdateData(
                table: "NoteTypes",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 2, 23, 52, 8, 605, DateTimeKind.Utc).AddTicks(9370), new DateTime(2026, 1, 2, 23, 52, 8, 605, DateTimeKind.Utc).AddTicks(9375) });

            migrationBuilder.UpdateData(
                table: "NoteTypes",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 2, 23, 52, 8, 606, DateTimeKind.Utc).AddTicks(1159), new DateTime(2026, 1, 2, 23, 52, 8, 606, DateTimeKind.Utc).AddTicks(1160) });

            migrationBuilder.CreateIndex(
                name: "IX_Decks_Name_Id",
                table: "Decks",
                columns: new[] { "Name", "Id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserBotProviders_UserId",
                table: "UserBotProviders",
                column: "UserId");
        }
    }
}
