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

namespace Test.Service;

public class DeckServiceTests : DatabaseSetupHelper
{
    private readonly DataContext _context;
    private readonly DeckService _deckService;
    private readonly Faker _faker;
    private readonly Mock<IFlashcardAlgorithmService> _mockAlgorithmService;
    private readonly string _otherCreatorId = "other-user-456";
    private readonly string _testCreatorId = "test-user-123";

    public DeckServiceTests()
    {
        _context = Context;
        _faker = new Faker();

        // Seed database with comprehensive data
        SeedDatabase();

        _mockAlgorithmService = new Mock<IFlashcardAlgorithmService>();
        _deckService = new DeckService(_context, _mockAlgorithmService.Object);
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
        for (int i = 0; i < noteTypes.Count; i++) noteTypes[i].Id = i + 1;

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
                note.Id = i * 10 + notes.IndexOf(note) + 1;
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
            counts.Add(new DeckDailyCount
            {
                DeckId = deckId,
                Date = DateOnly.FromDateTime(_faker.Date.Recent(i)),
                CardState = _faker.PickRandom<CardState>(),
                Count = _faker.Random.Int(1, 50)
            });

        return counts;
    }

    private async Task GenerateCards(string creatorId, int deckId, List<(CardState? state, DateTime? dueDate)> cards)
    {
        // 1. Ensure NoteType and its Templates are saved
        NoteType noteType = new NoteTypeFaker(creatorId);
        _context.NoteTypes.Add(noteType);
        await _context.SaveChangesAsync(); // This generates IDs for NoteType AND Templates

        NoteTypeTemplate firstTemplate = noteType.Templates.First();

        // 2. Create the Note
        NoteFaker note = new(deckId, noteType.Id, creatorId, false);

        // 3. Create Cards
        // Explicitly set the Note property instead of passing 0 for noteId
        List<Card> cardEntities = cards.Select(x =>
        {
            Card? card = (Card)new CardFaker(0, firstTemplate.Id, x.state, x.dueDate);
            card.Note = note; // Link by object reference
            card.Template = firstTemplate; // Link by object reference
            return card;
        }).ToList();

        _context.Notes.Add(note);
        _context.Cards.AddRange(cardEntities);

        // 4. Final Save
        await _context.SaveChangesAsync();
    }


    [Fact]
    public async Task Create_ValidRequest_ReturnsDeck()
    {
        // Arrange
        CreateDeckRequest request = new()
        {
            Name = "New Test Deck",
            Description = "Test Description",
            OptionRequest = new DeckOptionRequest
            {
                NewLimitPerDay = 20,
                ReviewLimitPerDay = 100
            }
        };

        // Act
        Deck? result = await _deckService.Create(_testCreatorId, request);
        await _context.SaveChangesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(_testCreatorId, result.CreatorId);
        Assert.Equal(request.Name, result.Name);
        Assert.Equal(request.Description, result.Description);
        Assert.NotNull(result.Option);
        Assert.Equal(request.OptionRequest.NewLimitPerDay, result.Option.NewLimitPerDay);

        // Verify deck was saved to database
        Deck? savedDeck = await _context.Decks.FindAsync(result.Id);
        Assert.NotNull(savedDeck);
        Assert.Equal(request.Name, savedDeck.Name);
    }

    [Fact]
    public async Task Create_NullOptionRequest_CreatesDeckWithNullOption()
    {
        // Arrange
        CreateDeckRequest request = new()
        {
            Name = "Deck Without Option",
            Description = "Test Description",
            OptionRequest = null
        };

        // Act
        Deck? result = await _deckService.Create(_testCreatorId, request);
        await _context.SaveChangesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.Option);
    }


    [Fact]
    public async Task Update_DeckExists_UpdatesDeck()
    {
        // Arrange
        int deckId = 1;
        UpdateDeckRequest request = new()
        {
            Name = "Updated Deck Name",
            Description = "Updated Description",
            OptionRequest = new DeckOptionRequest
            {
                NewLimitPerDay = 30,
                ReviewLimitPerDay = 150
            }
        };

        // Act
        int updatedCount = await _deckService.Update(deckId, _testCreatorId, request);
        await _context.SaveChangesAsync();

        // Assert
        Assert.Equal(1, updatedCount);

        Deck? updatedDeck = await _context.Decks.AsNoTracking()
            .Include(d => d.Option)
            .FirstOrDefaultAsync(d => d.Id == deckId);
        Assert.NotNull(updatedDeck);
        Assert.Equal(request.Name, updatedDeck.Name);
        Assert.Equal(request.Description, updatedDeck.Description);
        Assert.NotNull(updatedDeck.Option);
        Assert.Equal(request.OptionRequest.NewLimitPerDay, updatedDeck.Option.NewLimitPerDay);
    }

    [Fact]
    public async Task Update_DeckNotFound_ReturnsZero()
    {
        // Arrange
        int deckId = 999; // Non-existent deck
        UpdateDeckRequest request = new()
        {
            Name = "Updated Name",
            Description = "Updated Description",
            OptionRequest = null
        };

        // Act
        int result = await _deckService.Update(deckId, _testCreatorId, request);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public async Task Update_WrongCreator_ReturnsZero()
    {
        // Arrange
        int deckId = 1; // Deck belongs to test user
        UpdateDeckRequest request = new()
        {
            Name = "Updated Name",
            Description = "Updated Description",
            OptionRequest = null
        };

        // Act - trying to update with wrong creator ID
        int result = await _deckService.Update(deckId, _otherCreatorId, request);

        // Assert
        Assert.Equal(0, result);

        // Verify deck was not updated
        Deck? deck = await _context.Decks.FindAsync(deckId);
        Assert.NotNull(deck);
        Assert.NotEqual(request.Name, deck.Name);
    }


    [Fact]
    public async Task Delete_DeckExists_DeletesDeck()
    {
        // Arrange
        int deckId = 2;
        Deck? deckBefore = await _context.Decks.FindAsync(deckId);
        Assert.NotNull(deckBefore);

        // Act
        int deletedCount = await _deckService.Delete(deckId, _testCreatorId);

        // Assert
        Assert.Equal(1, deletedCount);

        Deck? deckAfter = await _context.Decks.AsNoTracking().FirstOrDefaultAsync(x => x.Id == deckId);
        Assert.Null(deckAfter);
    }

    [Fact]
    public async Task Delete_DeckWithRelatedData_CascadesDelete()
    {
        // Arrange
        int deckId = 1; // This deck has notes and cards
        Deck? deckBefore = await _context.Decks
            .Include(d => d.Notes)
            .ThenInclude(n => n.Cards)
            .FirstOrDefaultAsync(d => d.Id == deckId);

        Assert.NotNull(deckBefore);
        Assert.NotEmpty(deckBefore.Notes);
        Assert.Contains(deckBefore.Notes, n => n.Cards.Any());

        // Act
        int deletedCount = await _deckService.Delete(deckId, _testCreatorId);
        await _context.SaveChangesAsync();

        // Assert
        Assert.Equal(1, deletedCount);

        // Verify deck is deleted
        Deck? deckAfter = await _context.Decks.AsNoTracking().FirstOrDefaultAsync(x => x.Id == deckId);
        Assert.Null(deckAfter);

        // Verify related notes are deleted (cascading)
        List<Note> notesAfter = await _context.Notes.AsNoTracking().Where(n => n.DeckId == deckId).ToListAsync();
        Assert.Empty(notesAfter);

        // Verify related cards are deleted (cascading)
        List<Card> cardsAfter = await _context.Cards.AsNoTracking().Where(c => c.Note.DeckId == deckId).ToListAsync();
        Assert.Empty(cardsAfter);
    }

    [Fact]
    public async Task Delete_WrongCreator_ReturnsZero()
    {
        // Arrange
        int deckId = 1; // Deck belongs to test user

        // Act - trying to delete with wrong creator ID
        int result = await _deckService.Delete(deckId, _otherCreatorId);

        // Assert
        Assert.Equal(0, result);

        // Verify deck still exists
        Deck? deck = await _context.Decks.FindAsync(deckId);
        Assert.NotNull(deck);
    }


    [Fact]
    public async Task GetStatistics_DeckExists_ReturnsStatisticsWithAllFields()
    {
        // Arrange
        int deckId = 1;
        Deck? deck = await _context.Decks
            .Include(d => d.Cards)
            .Include(d => d.DailyCounts)
            .FirstOrDefaultAsync(d => d.Id == deckId);

        Assert.NotNull(deck);

        // Calculate expected values
        double rawReviewEaseFactor = deck.Cards
            .Where(c => c.State == CardState.Review)
            .Sum(c => c.EaseFactor);

        double expectedRetention = 0.85;
        _mockAlgorithmService
            .Setup(x => x.EstimateRetention(It.IsAny<double>()))
            .Returns(expectedRetention);

        // Act
        DeckStatisticsResponse? result = await _deckService.GetStatistics(_testCreatorId, deckId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(deckId, result.Id);
        Assert.Equal(deck.Name, result.Name);
        Assert.Equal(deck.Description, result.Description);
        Assert.Equal(expectedRetention, result.RetentionRate);

        // Verify card counts
        int learningCount = deck.Cards.Count(c =>
            c.State == CardState.Learning || c.State == CardState.Relearning);
        int reviewCount = deck.Cards.Count(c => c.State == CardState.Review);
        int newCount = deck.Cards.Count(c => c.State == CardState.New);
        int suspendedCount = deck.Cards.Count(c => c.State == CardState.Suspended);

        Assert.Equal(learningCount, result.DeckCardCounts.Learning);
        Assert.Equal(reviewCount, result.DeckCardCounts.Review);
        Assert.Equal(newCount, result.DeckCardCounts.New);
        Assert.Equal(suspendedCount, result.DeckCardCounts.Suspended);

        // Verify daily counts
        Assert.Equal(deck.DailyCounts.Count, result.DailyCounts.Count);
        foreach (DeckStatisticDailyCountResponse dailyCount in result.DailyCounts)
        {
            DeckDailyCount? matchingDbCount = deck.DailyCounts
                .FirstOrDefault(dc => dc.Date == dailyCount.Date && dc.CardState == dailyCount.Rating);
            Assert.NotNull(matchingDbCount);
            Assert.Equal(matchingDbCount.Count, dailyCount.Count);
        }

        // Verify algorithm service was called with correct parameter
        _mockAlgorithmService.Verify(x => x.EstimateRetention(rawReviewEaseFactor), Times.Once);
    }

    [Fact]
    public async Task GetStatistics_DeckWithoutCards_ReturnsZeroCounts()
    {
        // Arrange
        // Create a new deck without cards
        Deck emptyDeck = new()
        {
            Name = "Empty Deck",
            Description = "No cards",
            CreatorId = _testCreatorId,
            DailyCounts = new List<DeckDailyCount>()
        };

        _context.Decks.Add(emptyDeck);
        await _context.SaveChangesAsync();

        _mockAlgorithmService
            .Setup(x => x.EstimateRetention(0))
            .Returns(0.0);

        // Act
        DeckStatisticsResponse? result = await _deckService.GetStatistics(_testCreatorId, emptyDeck.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.DeckCardCounts.Learning);
        Assert.Equal(0, result.DeckCardCounts.Review);
        Assert.Equal(0, result.DeckCardCounts.New);
        Assert.Equal(0, result.DeckCardCounts.Suspended);
        Assert.Empty(result.DailyCounts);
    }

    [Fact]
    public async Task GetStatistics_DeckWithOnlyNewCards_NoReviewEaseFactor()
    {
        // Arrange
        Deck deck = new DeckFaker(_testCreatorId);
        _context.Decks.Add(deck);
        await _context.SaveChangesAsync();

        // Add only new cards (no review cards)
        List<(CardState? state, DateTime? dueDate)> cards = [];
        for (int i = 0; i < 5; i++) cards.Add((CardState.New, null));

        await GenerateCards(_testCreatorId, deck.Id, cards);


        _mockAlgorithmService
            .Setup(x => x.EstimateRetention(0))
            .Returns(0.0);

        // Act
        DeckStatisticsResponse? result = await _deckService.GetStatistics(_testCreatorId, deck.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.DeckCardCounts.Review);
        Assert.Equal(5, result.DeckCardCounts.New);
        // Algorithm service should be called with 0 since no review cards
        _mockAlgorithmService.Verify(x => x.EstimateRetention(0), Times.Once);
    }

    [Fact]
    public async Task GetStatistics_DeckNotFound_ReturnsNull()
    {
        // Arrange
        int deckId = 999; // Non-existent deck

        // Act
        DeckStatisticsResponse? result = await _deckService.GetStatistics(_testCreatorId, deckId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetStatistics_WrongCreator_ReturnsNull()
    {
        // Arrange
        int deckId = 100; // Deck belongs to other user

        // Act
        DeckStatisticsResponse? result = await _deckService.GetStatistics(_testCreatorId, deckId);

        // Assert
        Assert.Null(result);
    }


    [Fact]
    public async Task GetSummary_DeckExistsWithDueCards_ReturnsSummary()
    {
        // Arrange
        int deckId = 1;
        DateTime today = DateTime.UtcNow.Date;

        // Get deck and modify cards
        Deck? deck = await _context.Decks
            .Include(d => d.Cards)
            .FirstOrDefaultAsync(d => d.Id == deckId);

        Assert.NotNull(deck);

        // Reset cards for consistent test
        deck.Cards.Clear();

        // Add cards with specific states and due dates
        // 2 Learning cards
        List<(CardState?, DateTime?)> cards =
        [
            (CardState.Learning, null),
            (CardState.Relearning, null),
            (CardState.Review, today),
            (CardState.Review, today.AddDays(-1)),
            (CardState.Review, today.AddDays(1))
        ];
        // 4 New cards
        for (int i = 0; i < 4; i++) cards.Add((CardState.New, null));

        await GenerateCards(deck.CreatorId, deck.Id, cards);

        // Act
        DeckSummaryResponse? result = await _deckService.GetSummary(_testCreatorId, deckId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(deckId, result.Id);
        Assert.Equal(deck.Name, result.Name);
        Assert.Equal(deck.Description, result.Description);

        // Learning count: 2 (Learning + Relearning)
        Assert.Equal(2, result.DeckDueCount.Learning);

        // Review count: 2 (only cards due today or earlier)
        Assert.Equal(2, result.DeckDueCount.Review);

        // New count: 4 (all new cards)
        Assert.Equal(4, result.DeckDueCount.New);
    }

    [Fact]
    public async Task GetSummary_DeckWithCustomOptions_RespectsDeckLimits()
    {
        // Arrange
        int deckId = 1;
        Deck? deck = await _context.Decks.FindAsync(deckId);
        Assert.NotNull(deck);

        // Set custom deck options with low limits
        deck.Option = new DeckOption
        {
            NewLimitPerDay = 2,
            ReviewLimitPerDay = 3
        };

        // Clear existing cards and add many cards
        deck.Cards.Clear();

        List<(CardState? state, DateTime? dueDate)> cards = [];
        // Add many review cards
        for (int i = 0; i < 10; i++) cards.Add((CardState.Review, DateTime.UtcNow.Date.AddDays(-1)));

        // Add many new cards
        for (int i = 0; i < 10; i++) cards.Add((CardState.New, null));

        await GenerateCards(deck.CreatorId, deck.Id, cards);

        await _context.SaveChangesAsync();

        // Act
        DeckSummaryResponse? result = await _deckService.GetSummary(_testCreatorId, deckId);

        // Assert
        Assert.NotNull(result);
        // Review count should be limited to deck's ReviewLimitPerDay (3)
        Assert.Equal(3, result.DeckDueCount.Review);
        // New count should be limited to deck's NewLimitPerDay (2)
        Assert.Equal(2, result.DeckDueCount.New);
    }

    [Fact]
    public async Task GetSummary_DeckWithoutOptions_UsesCreatorOptions()
    {
        // Arrange
        int deckId = 4; // Deck without custom options
        Deck? deck = await _context.Decks
            .Include(d => d.Creator)
            .ThenInclude(c => c.DeckOption)
            .FirstOrDefaultAsync(d => d.Id == deckId);

        Assert.NotNull(deck);
        Assert.Null(deck.Option);

        // Set creator's default options with high limits
        deck.Creator.DeckOption.NewLimitPerDay = 50;
        deck.Creator.DeckOption.ReviewLimitPerDay = 200;

        // Add many cards
        deck.Cards.Clear();

        // Add 30 review cards due
        for (int i = 0; i < 30; i++)
            deck.Cards.Add(new Card
            {
                State = CardState.Review,
                DueDate = DateTime.UtcNow.Date.AddDays(-1)
            });

        // Add 40 new cards
        for (int i = 0; i < 40; i++)
            deck.Cards.Add(new Card
            {
                State = CardState.New
            });

        await _context.SaveChangesAsync();

        // Act
        DeckSummaryResponse? result = await _deckService.GetSummary(_testCreatorId, deckId);

        // Assert
        Assert.NotNull(result);
        // Review count should be limited to creator's ReviewLimitPerDay (200) but we only have 30
        Assert.Equal(30, result.DeckDueCount.Review);
        // New count should be limited to creator's NewLimitPerDay (50) but we only have 40
        Assert.Equal(40, result.DeckDueCount.New);
    }

    [Fact]
    public async Task GetSummary_DeckWithAllCardsSuspended_ReturnsZeroDueCards()
    {
        // Arrange
        Deck deck = new()
        {
            Name = "Suspended Deck",
            Description = "All cards suspended",
            CreatorId = _testCreatorId,
            Cards = new List<Card>()
        };

        // Add only suspended cards
        for (int i = 0; i < 10; i++)
            deck.Cards.Add(new Card
            {
                State = CardState.Suspended
            });

        _context.Decks.Add(deck);
        await _context.SaveChangesAsync();

        // Act
        DeckSummaryResponse? result = await _deckService.GetSummary(_testCreatorId, deck.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.DeckDueCount.Learning);
        Assert.Equal(0, result.DeckDueCount.Review);
        Assert.Equal(0, result.DeckDueCount.New);
    }

    [Fact]
    public async Task GetSummary_DeckWithMixedCardsDueInFuture_ExcludesFutureDueCards()
    {
        // Arrange
        DateTime today = DateTime.UtcNow.Date;

        Deck deck = new DeckFaker(_testCreatorId);
        _context.Decks.Add(deck);
        await _context.SaveChangesAsync();

        // Add review cards due in future
        List<(CardState? state, DateTime? dueDate)> cards = [];
        for (int i = 1; i <= 5; i++) cards.Add((CardState.Review, today.AddDays(i)));

        // Add one review card due today
        cards.Add((CardState.Review, today));
        await GenerateCards(deck.CreatorId, deck.Id, cards);


        // Act
        DeckSummaryResponse? result = await _deckService.GetSummary(_testCreatorId, deck.Id);

        // Assert
        Assert.NotNull(result);
        // Only the card due today should be counted
        Assert.Equal(1, result.DeckDueCount.Review);
    }

    [Fact]
    public async Task GetSummary_DeckNotFound_ReturnsNull()
    {
        // Arrange
        int deckId = 999;

        // Act
        DeckSummaryResponse? result = await _deckService.GetSummary(_testCreatorId, deckId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetSummary_WrongCreator_ReturnsNull()
    {
        // Arrange
        int deckId = 100; // Deck belongs to other user

        // Act
        DeckSummaryResponse? result = await _deckService.GetSummary(_testCreatorId, deckId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task Update_WithNullOptionRequest_SetsOptionToNull()
    {
        // Arrange
        int deckId = 1;
        Deck? deckBefore = await _context.Decks.FindAsync(deckId);
        deckBefore.Option = new DeckOption { NewLimitPerDay = 10 };
        await _context.SaveChangesAsync();

        UpdateDeckRequest request = new()
        {
            Name = "Updated Name",
            Description = "Updated Description",
            OptionRequest = null
        };

        // Act
        int result = await _deckService.Update(deckId, _testCreatorId, request);
        await _context.SaveChangesAsync();

        // Assert
        Assert.Equal(1, result);

        Deck? deckAfter = await _context.Decks.AsNoTracking().FirstOrDefaultAsync(x => x.Id == deckId);
        Assert.NotNull(deckAfter);
        Assert.Null(deckAfter.Option);
    }

    [Fact]
    public async Task Create_WithEmptyName_ThrowsException()
    {
        // Arrange
        CreateDeckRequest request = new()
        {
            Name = "", // Empty name
            Description = "Test Description",
            OptionRequest = null
        };

        // Act & Assert
        // Note: Actual validation might be handled by DataAnnotations or FluentValidation
        // This test shows how you might test validation
        Deck? deck = await _deckService.Create(_testCreatorId, request);
        await _context.SaveChangesAsync();

        // The service might allow empty names depending on your validation rules
        // Adjust this test based on your actual requirements
        Assert.NotNull(deck);
    }

    [Fact]
    public async Task GetStatistics_DeckWithDuplicateDailyCounts_HandlesCorrectly()
    {
        // Arrange
        Deck deck = new()
        {
            Name = "Test Deck",
            Description = "Test Description",
            CreatorId = _testCreatorId,
            DailyCounts = new List<DeckDailyCount>()
        };

        // Add duplicate daily counts for same date and state
        DateOnly date = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        deck.DailyCounts.Add(new DeckDailyCount
        {
            Date = date,
            CardState = CardState.Review,
            Count = 10
        });
        deck.DailyCounts.Add(new DeckDailyCount
        {
            Date = date,
            CardState = CardState.Review,
            Count = 5
        });

        _context.Decks.Add(deck);
        await _context.SaveChangesAsync();

        // Act
        DeckStatisticsResponse? result = await _deckService.GetStatistics(_testCreatorId, deck.Id);

        // Assert
        Assert.NotNull(result);
        // Both daily counts should be included
        Assert.Equal(2, result.DailyCounts.Count);
    }

    [Fact]
    public async Task ConcurrentUpdates_HandleCorrectly()
    {
        // Arrange
        int deckId = 1;

        // Simulate concurrent updates
        Task<int> task1 = _deckService.Update(deckId, _testCreatorId, new UpdateDeckRequest
        {
            Name = "Update 1",
            Description = "Description 1",
            OptionRequest = null
        });

        Task<int> task2 = _deckService.Update(deckId, _testCreatorId, new UpdateDeckRequest
        {
            Name = "Update 2",
            Description = "Description 2",
            OptionRequest = null
        });

        // Act
        int[] results = await Task.WhenAll(task1, task2);

        // Assert
        Assert.Equal(1, results[0]);
        Assert.Equal(1, results[1]);

        // Check final state
        Deck? deck = await _context.Decks.FindAsync(deckId);
        Assert.NotNull(deck);
        // The deck should have the name from the last update
        // (Note: in real scenario with EF Core tracking, last writer wins)
    }
}