using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedingDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "NoteTypes",
                columns: new[] { "Id", "CreatedAt", "CreatorId", "CssStyle", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 12, 29, 5, 24, 38, 770, DateTimeKind.Utc).AddTicks(4409), null, ".card { font-family: arial; text-align: center; }", "Basic", new DateTime(2025, 12, 29, 5, 24, 38, 770, DateTimeKind.Utc).AddTicks(4413) },
                    { 2, new DateTime(2025, 12, 29, 5, 24, 38, 770, DateTimeKind.Utc).AddTicks(5898), null, ".card { font-family: arial; text-align: center; }", "Basic (and reversed card)", new DateTime(2025, 12, 29, 5, 24, 38, 770, DateTimeKind.Utc).AddTicks(5898) }
                });

            migrationBuilder.InsertData(
                table: "NoteTypeTemplates",
                columns: new[] { "Id", "Back", "CreatedAt", "Front", "NoteTypeId", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "{{Front}}<hr id=answer>{{Back}}", new DateTime(2025, 12, 29, 5, 24, 38, 771, DateTimeKind.Utc).AddTicks(6131), "{{Front}}", 1, new DateTime(2025, 12, 29, 5, 24, 38, 771, DateTimeKind.Utc).AddTicks(6136) },
                    { 2, "{{Front}}<hr id=answer>{{Back}}", new DateTime(2025, 12, 29, 5, 24, 38, 771, DateTimeKind.Utc).AddTicks(7508), "{{Front}}", 2, new DateTime(2025, 12, 29, 5, 24, 38, 771, DateTimeKind.Utc).AddTicks(7509) },
                    { 3, "{{Back}}<hr id=answer>{{Front}}", new DateTime(2025, 12, 29, 5, 24, 38, 771, DateTimeKind.Utc).AddTicks(7512), "{{Back}}", 2, new DateTime(2025, 12, 29, 5, 24, 38, 771, DateTimeKind.Utc).AddTicks(7512) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "NoteTypeTemplates",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "NoteTypeTemplates",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "NoteTypeTemplates",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "NoteTypes",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "NoteTypes",
                keyColumn: "Id",
                keyValue: 2);
        }
    }
}
