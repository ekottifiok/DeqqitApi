#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedUserBot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserBotCodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RandomCode = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserBotCodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserBotCodes_AspNetUsers_UserId",
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

            migrationBuilder.CreateIndex(
                name: "IX_UserBotCodes_UserId",
                table: "UserBotCodes",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserBotCodes");

            migrationBuilder.UpdateData(
                table: "NoteTypeTemplates",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 29, 5, 24, 38, 771, DateTimeKind.Utc).AddTicks(6131), new DateTime(2025, 12, 29, 5, 24, 38, 771, DateTimeKind.Utc).AddTicks(6136) });

            migrationBuilder.UpdateData(
                table: "NoteTypeTemplates",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 29, 5, 24, 38, 771, DateTimeKind.Utc).AddTicks(7508), new DateTime(2025, 12, 29, 5, 24, 38, 771, DateTimeKind.Utc).AddTicks(7509) });

            migrationBuilder.UpdateData(
                table: "NoteTypeTemplates",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 29, 5, 24, 38, 771, DateTimeKind.Utc).AddTicks(7512), new DateTime(2025, 12, 29, 5, 24, 38, 771, DateTimeKind.Utc).AddTicks(7512) });

            migrationBuilder.UpdateData(
                table: "NoteTypes",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 29, 5, 24, 38, 770, DateTimeKind.Utc).AddTicks(4409), new DateTime(2025, 12, 29, 5, 24, 38, 770, DateTimeKind.Utc).AddTicks(4413) });

            migrationBuilder.UpdateData(
                table: "NoteTypes",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 29, 5, 24, 38, 770, DateTimeKind.Utc).AddTicks(5898), new DateTime(2025, 12, 29, 5, 24, 38, 770, DateTimeKind.Utc).AddTicks(5898) });
        }
    }
}
