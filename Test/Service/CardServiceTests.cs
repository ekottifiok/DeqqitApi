using Core.Data;
using Core.Dto.Card;
using Core.Dto.Common;
using Core.Model;
using Core.Model.Helper;
using Core.Services;
using Core.Services.Helper;
using Core.Services.Helper.Interface;
using Core.Services.Interface;
using Microsoft.EntityFrameworkCore;
using Moq;
using Test.Helper;
using Xunit;

namespace Test.Service;

public class CardServiceTests(CardServiceFixture fixture) : IntegrationTestBase<CardServiceFixture>(fixture)
{
    private CardService _cardService;
    private Mock<ITemplateService> _mockTemplate;
    private Mock<IFlashcardAlgorithmService> _mockAlgo;
    private Mock<ITimeService> _mockTime;
    private Mock<IAiServiceFactory> _mockAiFactory;
    private Mock<IAiService> _mockAiService;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        
        _mockTemplate = new Mock<ITemplateService>();
        _mockTemplate.Setup(x => x.Parse(It.IsAny<string>(), It.IsAny<Dictionary<string,string>>()))
            .ReturnsAsync((string s, Dictionary<string,string> d) => $"Parsed {s}");

        _mockAlgo = new Mock<IFlashcardAlgorithmService>();
        _mockTime = new Mock<ITimeService>();
        _mockTime.Setup(x => x.UtcNow).Returns(new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc));

        _mockAiFactory = new Mock<IAiServiceFactory>();
        _mockAiService = new Mock<IAiService>();
        _mockAiFactory.Setup(x => x.GetUserService(It.IsAny<UserAiProvider>())).Returns(_mockAiService.Object);

        _cardService = new CardService(Context, _mockTemplate.Object, _mockAlgo.Object, _mockTime.Object, _mockAiFactory.Object);
    }

    [Fact]
    public async Task Get_ValidPagination_ReturnsCards()
    {
        PaginationRequest<int> request = new() { PageSize = 10 };
        PaginationResult<Card> result = await _cardService.Get(Fixture.TestCreatorId, Fixture.TestDeckId, request);

        Assert.NotNull(result);
        Assert.NotEmpty(result.Data);
        Assert.All(result.Data, c => Assert.Equal(Fixture.TestDeckId, c.Note.DeckId));
    }

    [Fact]
    public async Task Get_ValidId_ReturnsCard()
    {
        Card card = await Context.Cards.FirstAsync(c => c.Note.CreatorId == Fixture.TestCreatorId);
        ResponseResult<Card> result = await _cardService.Get(Fixture.TestCreatorId, card.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(card.Id, result.Value.Id);
    }

    [Fact]
    public async Task UpdateCardState_Suspend_SuspendsCard()
    {
        Card card = await Context.Cards.FirstAsync(c => c.Note.CreatorId == Fixture.TestCreatorId && c.State != CardState.Suspended);
        ResponseResult<bool> result = await _cardService.UpdateCardState(Fixture.TestCreatorId, card.Id, UpdateCardStateRequest.Suspend);

        Assert.True(result.IsSuccess);
        
        Context.ChangeTracker.Clear();
        Card? dbCard = await Context.Cards.FindAsync(card.Id);
        Assert.Equal(CardState.Suspended, dbCard.State);
    }

    [Fact]
    public async Task UpdateCardState_Reset_ResetsCard()
    {
        Card card = await Context.Cards.FirstAsync(c => c.Note.CreatorId == Fixture.TestCreatorId);
        // Ensure it's not new to verify change
        card.State = CardState.Learning;
        await Context.SaveChangesAsync();

        ResponseResult<bool> result = await _cardService.UpdateCardState(Fixture.TestCreatorId, card.Id, UpdateCardStateRequest.Reset);

        Assert.True(result.IsSuccess);
        
        Context.ChangeTracker.Clear();
        Card? dbCard = await Context.Cards.FindAsync(card.Id);
        Assert.Equal(CardState.New, dbCard.State);
        Assert.Equal(0, dbCard.Repetitions);
    }

    [Fact]
    public async Task GetNextStudyCard_PrioritizesLearning()
    {
        // Ensure we have a Learning card due now
        Card learningCard = await Context.Cards.FirstAsync(c => c.State == CardState.Learning);
        learningCard.DueDate = _mockTime.Object.UtcNow.AddMinutes(-1);
        await Context.SaveChangesAsync();

        ResponseResult<CardResponse> result = await _cardService.GetNextStudyCard(Fixture.TestCreatorId, Fixture.TestDeckId);

        Assert.True(result.IsSuccess);
        Assert.Equal(learningCard.Id, result.Value.Id);
        Assert.Equal(CardState.Learning, result.Value.State);
    }

    [Fact]
    public async Task SubmitCardReview_ValidRequest_UpdatesCard()
    {
        // 1. Arrange
        User? user = await Context.Users.FindAsync(Fixture.TestCreatorId);
        UserAiProvider provider = new UserAiProvider { Key = "key", Type = UserAiProviderType.ChatGpt };
        user.AiProviders.Add(provider);
        await Context.SaveChangesAsync();

        Card card = await Context.Cards.FirstAsync(c => c.Note.CreatorId == Fixture.TestCreatorId && c.State == CardState.New);
        
        _mockAiService.Setup(x => x.CheckAnswer(It.IsAny<string>(), "Answer", It.IsAny<string>()))
            .ReturnsAsync(5); // 5 = Perfect

        _mockAlgo.Setup(x => x.Calculate(5, CardState.New, It.IsAny<int>(), It.IsAny<double>(), It.IsAny<int>()))
            .Returns(new FlashcardResult { State = CardState.Review, Interval = 1, EaseFactor = 2.5, Repetitions = 1 });

        CardSubmitRequest request = new() { Answer = "Answer", UserProviderId = provider.Id };

        // 2. Act
        ResponseResult<CardResponse> result = await _cardService.SubmitCardReview(Fixture.TestCreatorId, card.Id, request);

        // 3. Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(CardState.Review, result.Value.State); // Response mapping might use updated entity or old? 
        // CardService maps AFTER saving, so it should be new state.
        
        Context.ChangeTracker.Clear();
        Card? dbCard = await Context.Cards.FindAsync(card.Id);
        Assert.Equal(CardState.Review, dbCard.State);
        Assert.Equal(1, dbCard.Repetitions);
    }
}
