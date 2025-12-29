using System.Text.Json;
using System.Text.Json.Serialization;
using Bogus;
using Core.Data;
using Core.Dto.Deck;
using Core.Model;
using Core.Model.Faker;
using Core.Model.Helper;
using Core.Services;
using Core.Services.Helper.Interface;
using Microsoft.EntityFrameworkCore;
using Moq;
using Test.Helper;

namespace ConsoleApp;

class DeckServiceTestFaker : DatabaseSetupHelper
{
    private readonly DataContext _context;
    private readonly Mock<IFlashcardAlgorithmService> _mockAlgorithmService;
    private readonly DeckService _deckService;
    private readonly string _testCreatorId = "test-user-123";
    private readonly string _otherCreatorId = "other-user-456";
    private readonly Faker _faker;

    public DeckServiceTestFaker()
    {
        _context = Context;
        _faker = new Faker();

        // Seed database with comprehensive data
        SeedDatabase();

        _mockAlgorithmService = new Mock<IFlashcardAlgorithmService>();
        _deckService = new DeckService(_context, _mockAlgorithmService.Object);
    }

    public void PrintSqlCommand()
    {
        _sqlLog.ForEach(Console.WriteLine);
    }

    private void SeedDatabase()
    {
        // Create users
        User? testUser = new UserFaker(_testCreatorId).Generate();
        testUser.Id = _testCreatorId;
        testUser.DeckOption = new DeckOptionFaker().Generate();

        User? otherUser = new UserFaker(_otherCreatorId).Generate();
        otherUser.Id = _otherCreatorId;
        otherUser.DeckOption = new DeckOptionFaker().Generate();

        _context.Users.AddRange(testUser, otherUser);

        // Create note types for test user
        NoteTypeFaker noteTypeFaker = new(_testCreatorId);
        List<NoteType>? noteTypes = noteTypeFaker.Generate(2);
        for (int i = 0; i < noteTypes.Count; i++)
        {
            noteTypes[i].Id = i + 1;
        }

        _context.NoteTypes.AddRange(noteTypes);

        // Create decks for test user
        DeckFaker deckFaker = new(_testCreatorId);
        List<Deck>? decks = deckFaker.Generate(5);
        for (int i = 0; i < decks.Count; i++)
        {
            decks[i].Id = i + 1;
            decks[i].Creator = testUser;

            switch (i)
            {
                // Add custom deck option to some decks
                case < 2:
                    decks[i].Option = new DeckOptionFaker().Generate();
                    break;
                // Add notes and cards to some decks
                case >= 3:
                    continue;
            }

            // Add notes to deck
            NoteFaker noteFaker = new(decks[i].Id, 1, _testCreatorId);
            List<Note>? notes = noteFaker.Generate(3);

            foreach (Note note in notes)
            {
                note.Id = (i * 10) + notes.IndexOf(note) + 1;
                decks[i].Notes.Add(note);

                // Ensure cards are properly linked to their note
                foreach (Card card in note.Cards)
                {
                    card.NoteId = note.Id;
                    card.NoteTypeTemplateId = 1; // Use first template
                }
            }

            // Add daily counts
            decks[i].DailyCounts = GenerateDailyCounts(decks[i].Id);
        }

        // Create decks for other user
        DeckFaker otherDeckFaker = new(_otherCreatorId);
        List<Deck>? otherUserDecks = otherDeckFaker.Generate(2);
        for (int i = 0; i < otherUserDecks.Count; i++)
        {
            otherUserDecks[i].Id = 100 + i;
            otherUserDecks[i].Creator = otherUser;
        }

        _context.Decks.AddRange(decks);
        _context.Decks.AddRange(otherUserDecks);

        _context.SaveChanges();
    }

    private ICollection<DeckDailyCount> GenerateDailyCounts(int deckId)
    {
        List<DeckDailyCount> counts = [];
        int days = _faker.Random.Int(3, 7);

        for (int i = 0; i < days; i++)
        {
            counts.Add(new DeckDailyCount
            {
                DeckId = deckId,
                Date = DateOnly.FromDateTime(_faker.Date.Recent(i)),
                CardState = _faker.PickRandom<CardState>(),
                Count = _faker.Random.Int(1, 50)
            });
        }

        return counts;
    }


    private async Task GenerateCards(string creatorId, int deckId, List<(CardState? state, DateTime? dueDate)> cards)
    {
        NoteType noteType = new NoteTypeFaker(creatorId);
        _context.NoteTypes.Add(noteType);
        await _context.SaveChangesAsync();

        NoteTypeTemplate firstTemplate = noteType.Templates.First();
        Note note = new NoteFaker(deckId, noteType.Id, creatorId, false);
        note.Cards = cards.Select(x =>
        {
            Card? card = (Card)new CardFaker(0, firstTemplate.Id, x.state, x.dueDate);
            return card;
        }).ToList();

        _context.Notes.Add(note);
        await _context.SaveChangesAsync();
    }


    public async Task GetSummary_DeckWithMixedCardsDueInFuture_ExcludesFutureDueCards()
    {
        // Arrange
        DateTime today = DateTime.UtcNow.Date;

        Deck deck = new DeckFaker(_testCreatorId);
        _context.Decks.Add(deck);
        await _context.SaveChangesAsync();

        // Add review cards due in future
        List<(CardState? state, DateTime? dueDate)> cards = [];
        for (int i = 1; i <= 5; i++)
        {
            cards.Add((CardState.Review, today.AddDays(i)));
        }

        // Add one review card due today
        cards.Add((CardState.Review, today));
        await GenerateCards(deck.CreatorId, deck.Id, cards);


        // Act
        DeckSummaryResponse? result = await _deckService.GetSummary(_testCreatorId, deck.Id);
        var resultDecks = await _context.Cards.Where(x => x.Note.DeckId == deck.Id)
            .Select(x => new { x.Id, x.State, x.DueDate }).ToListAsync();
        JsonSerializerOptions jsonSerializerOptions = new()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };

        Console.WriteLine(JsonSerializer.Serialize(result, jsonSerializerOptions));
        Console.WriteLine(JsonSerializer.Serialize(resultDecks, jsonSerializerOptions));
        Console.WriteLine($"TODAY: {today.ToUniversalTime()}");
    }
}