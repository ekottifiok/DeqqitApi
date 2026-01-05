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

public class NoteServiceFixture : DatabaseFixture
{
    public string TestCreatorId { get; } = "note-tester";
    public int TestDeckId { get; private set; }
    public int TestNoteTypeId { get; private set; }

    protected override async Task SeedAsync(DataContext context)
    {
        // 1. User
        User? user = new UserFaker("NoteTester")
            .RuleFor(u => u.Id, _ => TestCreatorId)
            .Generate();
        context.Users.Add(user);

        // 2. Deck
        Deck? deck = new DeckFaker(TestCreatorId).Generate();
        context.Decks.Add(deck);
        await context.SaveChangesAsync();
        TestDeckId = deck.Id;

        // 3. NoteType
        NoteType? noteType = new NoteTypeFaker(TestCreatorId).Generate();
        context.NoteTypes.Add(noteType);
        await context.SaveChangesAsync();
        TestNoteTypeId = noteType.Id;
    }
}
