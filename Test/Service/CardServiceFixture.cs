using Core.Data;
using Core.Model;
using Core.Model.Faker;
using Core.Model.Helper;
using Core.Services;
using Core.Services.Helper.Interface;
using Microsoft.EntityFrameworkCore;
using Moq;
using Test.Helper;
using Xunit;

namespace Test.Service;

public class CardServiceFixture : DatabaseFixture
{
    public string TestCreatorId { get; } = "card-tester";
    public int TestDeckId { get; private set; }
    public int TestNoteTypeId { get; private set; }
    public int TestTemplateId { get; private set; }

    protected override async Task SeedAsync(DataContext context)
    {
        // 1. User
        User? user = new UserFaker("CardTester")
            .RuleFor(u => u.Id, _ => TestCreatorId)
            .Generate();
        context.Users.Add(user);

        // 2. Deck
        Deck? deck = new DeckFaker(TestCreatorId)
            .RuleFor(d => d.Option, _ => new DeckOption { NewLimitPerDay = 10, ReviewLimitPerDay = 50 })
            .Generate();
        context.Decks.Add(deck);
        await context.SaveChangesAsync();
        TestDeckId = deck.Id;

        // 3. NoteType
        NoteType? noteType = new NoteTypeFaker(TestCreatorId).Generate();
        context.NoteTypes.Add(noteType);
        await context.SaveChangesAsync();
        TestNoteTypeId = noteType.Id;
        TestTemplateId = noteType.Templates.First().Id;

        // 4. Notes & Cards
        // We need explicit control over Cards for specific tests, but we can seed some defaults
        Note? note = new NoteFaker(deck.Id, noteType.Id, TestCreatorId, shouldGenerateCard: false)
            .Generate();
        context.Notes.Add(note);
        await context.SaveChangesAsync();

        // Add specific cards for general retrieval
        List<Card> cards = new List<Card>
        {
            new CardFaker(note.Id, TestTemplateId, CardState.New).Generate(),
            new CardFaker(note.Id, TestTemplateId, CardState.Review).Generate(),
            new CardFaker(note.Id, TestTemplateId, CardState.Learning).Generate()
        };
        
        foreach(Card c in cards) deck.Cards.Add(c); // Ensure link
        context.Cards.AddRange(cards);
        await context.SaveChangesAsync();
    }
}
