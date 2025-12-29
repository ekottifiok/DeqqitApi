using Core.Data.Helper;
using Core.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Core.Data;

public class DataContext(DbContextOptions<DataContext> options) : IdentityDbContext<User>(options)
{
    public DbSet<Deck> Decks { get; set; }
    public DbSet<DeckDailyCount> DailyCounts { get; set; }
    public DbSet<NoteType> NoteTypes { get; set; }
    public DbSet<NoteTypeTemplate> NoteTypeTemplates { get; set; }
    public DbSet<Note> Notes { get; set; }
    public DbSet<Card> Cards { get; set; }
    public DbSet<UserRefreshToken> UserRefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 1. Seed NoteTypes
        modelBuilder.Entity<NoteType>().HasData(
            new NoteType 
            { 
                Id = 1, 
                Name = "Basic", 
                CssStyle = ".card { font-family: arial; text-align: center; }",
                CreatorId = null // System default
            },
            new NoteType 
            { 
                Id = 2, 
                Name = "Basic (and reversed card)", 
                CssStyle = ".card { font-family: arial; text-align: center; }",
                CreatorId = null 
            }
        );

        // 2. Seed NoteTypeTemplates
        modelBuilder.Entity<NoteTypeTemplate>().HasData(
            // Templates for "Basic" (Id 1)
            new NoteTypeTemplate 
            { 
                Id = 1, 
                NoteTypeId = 1, 
                Front = "{{Front}}", 
                Back = "{{Front}}<hr id=answer>{{Back}}" 
            },

            // Templates for "Basic (and reversed)" (Id 2)
            new NoteTypeTemplate 
            { 
                Id = 2, 
                NoteTypeId = 2, 
                Front = "{{Front}}", 
                Back = "{{Front}}<hr id=answer>{{Back}}" 
            },
            new NoteTypeTemplate 
            { 
                Id = 3, 
                NoteTypeId = 2, 
                Front = "{{Back}}", 
                Back = "{{Back}}<hr id=answer>{{Front}}" 
            }
        );
        
        modelBuilder.Entity<Note>().Property(b => b.Data).HasJsonConversion();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.AddInterceptors(
            new TimestampInterceptor()
        );
    }
}