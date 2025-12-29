#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Core.Data.Migrations;

/// <inheritdoc />
public partial class Initial : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            "AspNetRoles",
            table => new
            {
                Id = table.Column<string>("text", nullable: false),
                Name = table.Column<string>("character varying(256)", maxLength: 256, nullable: true),
                NormalizedName = table.Column<string>("character varying(256)", maxLength: 256, nullable: true),
                ConcurrencyStamp = table.Column<string>("text", nullable: true)
            },
            constraints: table => { table.PrimaryKey("PK_AspNetRoles", x => x.Id); });

        migrationBuilder.CreateTable(
            "AspNetUsers",
            table => new
            {
                Id = table.Column<string>("text", nullable: false),
                ProfileImageUrl = table.Column<string>("text", nullable: false),
                UserStreaks = table.Column<List<DateOnly>>("date[]", nullable: false),
                DeckOption_NewLimitPerDay = table.Column<int>("integer", nullable: false),
                DeckOption_ReviewLimitPerDay = table.Column<int>("integer", nullable: false),
                DeckOption_SortOrder = table.Column<int>("integer", nullable: false),
                DeckOption_InterdayLearningMix = table.Column<bool>("boolean", nullable: false),
                UserName = table.Column<string>("character varying(256)", maxLength: 256, nullable: true),
                NormalizedUserName = table.Column<string>("character varying(256)", maxLength: 256, nullable: true),
                Email = table.Column<string>("character varying(256)", maxLength: 256, nullable: true),
                NormalizedEmail = table.Column<string>("character varying(256)", maxLength: 256, nullable: true),
                EmailConfirmed = table.Column<bool>("boolean", nullable: false),
                PasswordHash = table.Column<string>("text", nullable: true),
                SecurityStamp = table.Column<string>("text", nullable: true),
                ConcurrencyStamp = table.Column<string>("text", nullable: true),
                PhoneNumber = table.Column<string>("text", nullable: true),
                PhoneNumberConfirmed = table.Column<bool>("boolean", nullable: false),
                TwoFactorEnabled = table.Column<bool>("boolean", nullable: false),
                LockoutEnd = table.Column<DateTimeOffset>("timestamp with time zone", nullable: true),
                LockoutEnabled = table.Column<bool>("boolean", nullable: false),
                AccessFailedCount = table.Column<int>("integer", nullable: false)
            },
            constraints: table => { table.PrimaryKey("PK_AspNetUsers", x => x.Id); });

        migrationBuilder.CreateTable(
            "AspNetRoleClaims",
            table => new
            {
                Id = table.Column<int>("integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy",
                        NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                RoleId = table.Column<string>("text", nullable: false),
                ClaimType = table.Column<string>("text", nullable: true),
                ClaimValue = table.Column<string>("text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                table.ForeignKey(
                    "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                    x => x.RoleId,
                    "AspNetRoles",
                    "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            "AspNetUserClaims",
            table => new
            {
                Id = table.Column<int>("integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy",
                        NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                UserId = table.Column<string>("text", nullable: false),
                ClaimType = table.Column<string>("text", nullable: true),
                ClaimValue = table.Column<string>("text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                table.ForeignKey(
                    "FK_AspNetUserClaims_AspNetUsers_UserId",
                    x => x.UserId,
                    "AspNetUsers",
                    "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            "AspNetUserLogins",
            table => new
            {
                LoginProvider = table.Column<string>("text", nullable: false),
                ProviderKey = table.Column<string>("text", nullable: false),
                ProviderDisplayName = table.Column<string>("text", nullable: true),
                UserId = table.Column<string>("text", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                table.ForeignKey(
                    "FK_AspNetUserLogins_AspNetUsers_UserId",
                    x => x.UserId,
                    "AspNetUsers",
                    "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            "AspNetUserRoles",
            table => new
            {
                UserId = table.Column<string>("text", nullable: false),
                RoleId = table.Column<string>("text", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                table.ForeignKey(
                    "FK_AspNetUserRoles_AspNetRoles_RoleId",
                    x => x.RoleId,
                    "AspNetRoles",
                    "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    "FK_AspNetUserRoles_AspNetUsers_UserId",
                    x => x.UserId,
                    "AspNetUsers",
                    "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            "AspNetUserTokens",
            table => new
            {
                UserId = table.Column<string>("text", nullable: false),
                LoginProvider = table.Column<string>("text", nullable: false),
                Name = table.Column<string>("text", nullable: false),
                Value = table.Column<string>("text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                table.ForeignKey(
                    "FK_AspNetUserTokens_AspNetUsers_UserId",
                    x => x.UserId,
                    "AspNetUsers",
                    "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            "Decks",
            table => new
            {
                Id = table.Column<int>("integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy",
                        NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                CreatorId = table.Column<string>("text", nullable: false),
                Name = table.Column<string>("text", nullable: false),
                Description = table.Column<string>("text", nullable: false),
                Option_NewLimitPerDay = table.Column<int>("integer", nullable: true),
                Option_ReviewLimitPerDay = table.Column<int>("integer", nullable: true),
                Option_SortOrder = table.Column<int>("integer", nullable: true),
                Option_InterdayLearningMix = table.Column<bool>("boolean", nullable: true),
                CreatedAt = table.Column<DateTime>("timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTime>("timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Decks", x => x.Id);
                table.ForeignKey(
                    "FK_Decks_AspNetUsers_CreatorId",
                    x => x.CreatorId,
                    "AspNetUsers",
                    "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            "NoteTypes",
            table => new
            {
                Id = table.Column<int>("integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy",
                        NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                CreatorId = table.Column<string>("text", nullable: true),
                Name = table.Column<string>("text", nullable: false),
                CssStyle = table.Column<string>("text", nullable: false),
                CreatedAt = table.Column<DateTime>("timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTime>("timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_NoteTypes", x => x.Id);
                table.ForeignKey(
                    "FK_NoteTypes_AspNetUsers_CreatorId",
                    x => x.CreatorId,
                    "AspNetUsers",
                    "Id");
            });

        migrationBuilder.CreateTable(
            "UserAiProvider",
            table => new
            {
                Id = table.Column<int>("integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy",
                        NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                UserId = table.Column<string>("text", nullable: false),
                Type = table.Column<int>("integer", nullable: false),
                Key = table.Column<string>("text", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserAiProvider", x => new { x.UserId, x.Id });
                table.ForeignKey(
                    "FK_UserAiProvider_AspNetUsers_UserId",
                    x => x.UserId,
                    "AspNetUsers",
                    "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            "UserRefreshTokens",
            table => new
            {
                Id = table.Column<Guid>("uuid", nullable: false),
                Token = table.Column<string>("text", nullable: false),
                Validity = table.Column<DateTime>("timestamp with time zone", nullable: false),
                UserId = table.Column<string>("text", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserRefreshTokens", x => x.Id);
                table.ForeignKey(
                    "FK_UserRefreshTokens_AspNetUsers_UserId",
                    x => x.UserId,
                    "AspNetUsers",
                    "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            "DailyCounts",
            table => new
            {
                Id = table.Column<int>("integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy",
                        NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                DeckId = table.Column<int>("integer", nullable: false),
                Date = table.Column<DateOnly>("date", nullable: false),
                CardState = table.Column<int>("integer", nullable: false),
                Count = table.Column<int>("integer", nullable: false),
                CreatedAt = table.Column<DateTime>("timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTime>("timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DailyCounts", x => x.Id);
                table.ForeignKey(
                    "FK_DailyCounts_Decks_DeckId",
                    x => x.DeckId,
                    "Decks",
                    "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            "Notes",
            table => new
            {
                Id = table.Column<int>("integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy",
                        NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                DeckId = table.Column<int>("integer", nullable: false),
                NoteTypeId = table.Column<int>("integer", nullable: false),
                CreatorId = table.Column<string>("text", nullable: false),
                Data = table.Column<string>("text", nullable: false),
                Tags = table.Column<List<string>>("text[]", nullable: false),
                CreatedAt = table.Column<DateTime>("timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTime>("timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Notes", x => x.Id);
                table.ForeignKey(
                    "FK_Notes_AspNetUsers_CreatorId",
                    x => x.CreatorId,
                    "AspNetUsers",
                    "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    "FK_Notes_Decks_DeckId",
                    x => x.DeckId,
                    "Decks",
                    "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    "FK_Notes_NoteTypes_NoteTypeId",
                    x => x.NoteTypeId,
                    "NoteTypes",
                    "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            "NoteTypeTemplates",
            table => new
            {
                Id = table.Column<int>("integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy",
                        NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                NoteTypeId = table.Column<int>("integer", nullable: false),
                Front = table.Column<string>("text", nullable: false),
                Back = table.Column<string>("text", nullable: false),
                CreatedAt = table.Column<DateTime>("timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTime>("timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_NoteTypeTemplates", x => x.Id);
                table.ForeignKey(
                    "FK_NoteTypeTemplates_NoteTypes_NoteTypeId",
                    x => x.NoteTypeId,
                    "NoteTypes",
                    "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            "Cards",
            table => new
            {
                Id = table.Column<int>("integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy",
                        NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                NoteId = table.Column<int>("integer", nullable: false),
                NoteTypeTemplateId = table.Column<int>("integer", nullable: false),
                State = table.Column<int>("integer", nullable: false),
                Interval = table.Column<int>("integer", nullable: false),
                Repetitions = table.Column<int>("integer", nullable: false),
                EaseFactor = table.Column<double>("double precision", nullable: false),
                DueDate = table.Column<DateTime>("timestamp with time zone", nullable: false),
                StepIndex = table.Column<int>("integer", nullable: false),
                DeckId = table.Column<int>("integer", nullable: true),
                CreatedAt = table.Column<DateTime>("timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTime>("timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Cards", x => x.Id);
                table.ForeignKey(
                    "FK_Cards_Decks_DeckId",
                    x => x.DeckId,
                    "Decks",
                    "Id");
                table.ForeignKey(
                    "FK_Cards_NoteTypeTemplates_NoteTypeTemplateId",
                    x => x.NoteTypeTemplateId,
                    "NoteTypeTemplates",
                    "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    "FK_Cards_Notes_NoteId",
                    x => x.NoteId,
                    "Notes",
                    "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            "IX_AspNetRoleClaims_RoleId",
            "AspNetRoleClaims",
            "RoleId");

        migrationBuilder.CreateIndex(
            "RoleNameIndex",
            "AspNetRoles",
            "NormalizedName",
            unique: true);

        migrationBuilder.CreateIndex(
            "IX_AspNetUserClaims_UserId",
            "AspNetUserClaims",
            "UserId");

        migrationBuilder.CreateIndex(
            "IX_AspNetUserLogins_UserId",
            "AspNetUserLogins",
            "UserId");

        migrationBuilder.CreateIndex(
            "IX_AspNetUserRoles_RoleId",
            "AspNetUserRoles",
            "RoleId");

        migrationBuilder.CreateIndex(
            "EmailIndex",
            "AspNetUsers",
            "NormalizedEmail");

        migrationBuilder.CreateIndex(
            "UserNameIndex",
            "AspNetUsers",
            "NormalizedUserName",
            unique: true);

        migrationBuilder.CreateIndex(
            "IX_Cards_DeckId",
            "Cards",
            "DeckId");

        migrationBuilder.CreateIndex(
            "IX_Cards_NoteId",
            "Cards",
            "NoteId");

        migrationBuilder.CreateIndex(
            "IX_Cards_NoteTypeTemplateId",
            "Cards",
            "NoteTypeTemplateId");

        migrationBuilder.CreateIndex(
            "IX_DailyCounts_DeckId",
            "DailyCounts",
            "DeckId");

        migrationBuilder.CreateIndex(
            "IX_Decks_CreatorId",
            "Decks",
            "CreatorId");

        migrationBuilder.CreateIndex(
            "IX_Decks_Name_Id",
            "Decks",
            ["Name", "Id"],
            unique: true);

        migrationBuilder.CreateIndex(
            "IX_Notes_CreatorId",
            "Notes",
            "CreatorId");

        migrationBuilder.CreateIndex(
            "IX_Notes_DeckId",
            "Notes",
            "DeckId");

        migrationBuilder.CreateIndex(
            "IX_Notes_NoteTypeId",
            "Notes",
            "NoteTypeId");

        migrationBuilder.CreateIndex(
            "IX_NoteTypes_CreatorId",
            "NoteTypes",
            "CreatorId");

        migrationBuilder.CreateIndex(
            "IX_NoteTypeTemplates_NoteTypeId",
            "NoteTypeTemplates",
            "NoteTypeId");

        migrationBuilder.CreateIndex(
            "IX_UserRefreshTokens_UserId",
            "UserRefreshTokens",
            "UserId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            "AspNetRoleClaims");

        migrationBuilder.DropTable(
            "AspNetUserClaims");

        migrationBuilder.DropTable(
            "AspNetUserLogins");

        migrationBuilder.DropTable(
            "AspNetUserRoles");

        migrationBuilder.DropTable(
            "AspNetUserTokens");

        migrationBuilder.DropTable(
            "Cards");

        migrationBuilder.DropTable(
            "DailyCounts");

        migrationBuilder.DropTable(
            "UserAiProvider");

        migrationBuilder.DropTable(
            "UserRefreshTokens");

        migrationBuilder.DropTable(
            "AspNetRoles");

        migrationBuilder.DropTable(
            "NoteTypeTemplates");

        migrationBuilder.DropTable(
            "Notes");

        migrationBuilder.DropTable(
            "Decks");

        migrationBuilder.DropTable(
            "NoteTypes");

        migrationBuilder.DropTable(
            "AspNetUsers");
    }
}