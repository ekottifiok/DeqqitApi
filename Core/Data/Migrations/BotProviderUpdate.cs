#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class BotProviderUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "IX_UserBotProviders_UserId",
                table: "UserBotProviders",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserBotProviders");

            migrationBuilder.UpdateData(
                table: "NoteTypeTemplates",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 2, 23, 31, 2, 70, DateTimeKind.Utc).AddTicks(6302), new DateTime(2026, 1, 2, 23, 31, 2, 70, DateTimeKind.Utc).AddTicks(6308) });

            migrationBuilder.UpdateData(
                table: "NoteTypeTemplates",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 2, 23, 31, 2, 70, DateTimeKind.Utc).AddTicks(7837), new DateTime(2026, 1, 2, 23, 31, 2, 70, DateTimeKind.Utc).AddTicks(7838) });

            migrationBuilder.UpdateData(
                table: "NoteTypeTemplates",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 2, 23, 31, 2, 70, DateTimeKind.Utc).AddTicks(7840), new DateTime(2026, 1, 2, 23, 31, 2, 70, DateTimeKind.Utc).AddTicks(7840) });

            migrationBuilder.UpdateData(
                table: "NoteTypes",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 2, 23, 31, 2, 69, DateTimeKind.Utc).AddTicks(5929), new DateTime(2026, 1, 2, 23, 31, 2, 69, DateTimeKind.Utc).AddTicks(5934) });

            migrationBuilder.UpdateData(
                table: "NoteTypes",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 2, 23, 31, 2, 69, DateTimeKind.Utc).AddTicks(7313), new DateTime(2026, 1, 2, 23, 31, 2, 69, DateTimeKind.Utc).AddTicks(7314) });
        }
    }
}
