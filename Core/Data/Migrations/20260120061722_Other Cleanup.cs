using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class OtherCleanup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Decks",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Decks",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "CreatorId",
                table: "Decks",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.UpdateData(
                table: "NoteTypeTemplates",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 20, 6, 17, 20, 448, DateTimeKind.Utc).AddTicks(7906), new DateTime(2026, 1, 20, 6, 17, 20, 448, DateTimeKind.Utc).AddTicks(7907) });

            migrationBuilder.UpdateData(
                table: "NoteTypeTemplates",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 20, 6, 17, 20, 448, DateTimeKind.Utc).AddTicks(8990), new DateTime(2026, 1, 20, 6, 17, 20, 448, DateTimeKind.Utc).AddTicks(8990) });

            migrationBuilder.UpdateData(
                table: "NoteTypeTemplates",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 20, 6, 17, 20, 448, DateTimeKind.Utc).AddTicks(8992), new DateTime(2026, 1, 20, 6, 17, 20, 448, DateTimeKind.Utc).AddTicks(8993) });

            migrationBuilder.UpdateData(
                table: "NoteTypes",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 20, 6, 17, 20, 447, DateTimeKind.Utc).AddTicks(7966), new DateTime(2026, 1, 20, 6, 17, 20, 447, DateTimeKind.Utc).AddTicks(7972) });

            migrationBuilder.UpdateData(
                table: "NoteTypes",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 20, 6, 17, 20, 447, DateTimeKind.Utc).AddTicks(9569), new DateTime(2026, 1, 20, 6, 17, 20, 447, DateTimeKind.Utc).AddTicks(9570) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Decks",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Decks",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AlterColumn<string>(
                name: "CreatorId",
                table: "Decks",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

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
        }
    }
}
