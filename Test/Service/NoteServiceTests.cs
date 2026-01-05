using Core.Data;
using Core.Dto.Common;
using Core.Dto.Note;
using Core.Model;
using Core.Model.Helper;
using Core.Services;
using Core.Services.Helper.Interface;
using Core.Services.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Test.Helper;
using Xunit;

namespace Test.Service;

public class NoteServiceTests(NoteServiceFixture fixture) : IntegrationTestBase<NoteServiceFixture>(fixture)
{
    private NoteService _noteService;
    private Mock<ITemplateService> _mockTemplate;
    private Mock<ILogger<NoteService>> _mockLogger;
    private Mock<IAiServiceFactory> _mockAiFactory;
    private Mock<IAiService> _mockAiService;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        
        _mockTemplate = new Mock<ITemplateService>();
        _mockTemplate.Setup(x => x.GetAllFields(It.IsAny<IEnumerable<string>>()))
            .Returns(new List<string> { "Front", "Back" });

        _mockLogger = new Mock<ILogger<NoteService>>();
        
        _mockAiFactory = new Mock<IAiServiceFactory>();
        _mockAiService = new Mock<IAiService>();
        _mockAiFactory.Setup(x => x.GetUserService(It.IsAny<UserAiProvider>())).Returns(_mockAiService.Object);

        _noteService = new NoteService(Context, _mockTemplate.Object, _mockLogger.Object, _mockAiFactory.Object);
    }

    [Fact]
    public async Task Create_ValidRequest_CreatesNoteAndCards()
    {
        // Arrange
        CreateNoteRequest request = new()
        {
            Data = new Dictionary<string, string> { { "Front", "Q" }, { "Back", "A" } },
            Tags = new List<string> { "tag1" }
        };
        List<CreateNoteRequest> requests = [request];

        // Act
        ResponseResult<bool> result = await _noteService.Create(Fixture.TestCreatorId, Fixture.TestDeckId, Fixture.TestNoteTypeId, requests);

        // Assert
        Assert.True(result.IsSuccess, result.Error?.Message);
        
        Context.ChangeTracker.Clear();
        Note? note = await Context.Notes.Include(n => n.Cards).FirstOrDefaultAsync(n => n.DeckId == Fixture.TestDeckId);
        Assert.NotNull(note);
        Assert.Equal("Q", note.Data["Front"]);
        Assert.NotEmpty(note.Cards); // Cards should be generated
    }

    [Fact]
    public async Task Get_ValidPagination_ReturnsNotes()
    {
        // Seed a note first
        Note note = new Note { CreatorId = Fixture.TestCreatorId, DeckId = Fixture.TestDeckId, NoteTypeId = Fixture.TestNoteTypeId, Data = new(), Tags = [] };
        Context.Notes.Add(note);
        await Context.SaveChangesAsync();

        PaginationRequest<int> request = new() { PageSize = 10 };
        PaginationResult<Note> result = await _noteService.Get(Fixture.TestCreatorId, Fixture.TestDeckId, request);

        Assert.NotNull(result);
        Assert.NotEmpty(result.Data);
        Assert.Contains(result.Data, n => n.Id == note.Id);
    }

    [Fact]
    public async Task GenerateFlashcards_ValidDescription_ReturnsFlashcards()
    {
        // Arrange
        User? user = await Context.Users.FindAsync(Fixture.TestCreatorId);
        UserAiProvider provider = new UserAiProvider { Key = "key", Type = UserAiProviderType.ChatGpt };
        user.AiProviders.Add(provider);
        await Context.SaveChangesAsync();

        _mockAiService.Setup(x => x.GenerateFlashcard(It.IsAny<List<string>>(), "Desc"))
            .ReturnsAsync(new List<Dictionary<string, string>> 
            { 
                new() { { "Front", "GenQ" }, { "Back", "GenA" } } 
            });

        // Act
        // Use generated provider ID
        ResponseResult<List<Dictionary<string, string>>> result = await _noteService.GenerateFlashcards(Fixture.TestCreatorId, provider.Id, Fixture.TestNoteTypeId, "Desc");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
        Assert.Equal("GenQ", result.Value[0]["Front"]);
    }
}
