using Core.Data;
using Core.Dto.Common;
using Core.Dto.NoteType;
using Core.Model;
using Core.Services;
using Core.Services.Helper.Interface;
using Core.Services.Interface;
using Microsoft.EntityFrameworkCore;
using Moq;
using Test.Helper;
using Xunit;

namespace Test.Service;

public class NoteTypeServiceTests(NoteTypeServiceFixture fixture) : IntegrationTestBase<NoteTypeServiceFixture>(fixture)
{
    private NoteTypeService _noteTypeService;
    private Mock<IHtmlService> _mockHtml;
    private Mock<ICssService> _mockCss;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        
        _mockHtml = new Mock<IHtmlService>();
        _mockHtml.Setup(x => x.Parse(It.IsAny<string>())).ReturnsAsync((string s) => s); // Pass through
        
        _mockCss = new Mock<ICssService>();
        _mockCss.Setup(x => x.Parse(It.IsAny<string>())).ReturnsAsync((string s) => s); // Pass through

        _noteTypeService = new NoteTypeService(Context, _mockHtml.Object, _mockCss.Object);
    }

    [Fact]
    public async Task Create_ValidRequest_CreatesNoteType()
    {
        CreateNoteTypeRequest request = new()
        {
            Name = "New Type",
            CssStyle = "body {}",
            Templates = new List<UpsertNoteTypeTemplateRequest>
            {
                new() { Front = "Front", Back = "Back" }
            }
        };

        ResponseResult<NoteType> result = await _noteTypeService.Create(Fixture.TestCreatorId, request);

        Assert.True(result.IsSuccess);
        Assert.Equal("New Type", result.Value.Name);
        Assert.Single(result.Value.Templates);
        
        Context.ChangeTracker.Clear();
        NoteType fromDb = await Context.NoteTypes.Include(nt => nt.Templates).FirstAsync(nt => nt.Name == "New Type");
        Assert.Single(fromDb.Templates);
    }

    [Fact]
    public async Task Get_ValidPagination_ReturnsNoteTypes()
    {
        // Seed
        Context.NoteTypes.Add(new NoteType { CreatorId = Fixture.TestCreatorId, Name = "Existing", Templates = [], CssStyle = "" });
        await Context.SaveChangesAsync();

        PaginationRequest<int> request = new() { PageSize = 10 };
        PaginationResult<NoteType> result = await _noteTypeService.Get(Fixture.TestCreatorId, request);

        Assert.NotNull(result);
        Assert.NotEmpty(result.Data); // Check Data property
    }

    [Fact]
    public async Task Delete_ValidId_DeletesNoteType()
    {
        NoteType nt = new NoteType { CreatorId = Fixture.TestCreatorId, Name = "To Delete", Templates = [], CssStyle = "" };
        Context.NoteTypes.Add(nt);
        await Context.SaveChangesAsync();

        ResponseResult<bool> result = await _noteTypeService.Delete(nt.Id, Fixture.TestCreatorId);

        Assert.True(result.IsSuccess);
        
        Context.ChangeTracker.Clear();
        Assert.Null(await Context.NoteTypes.FindAsync(nt.Id));
    }
    
    [Fact]
    public async Task Update_ThrowsNotImplemented()
    {
        await Assert.ThrowsAsync<NotImplementedException>(async () => 
            await _noteTypeService.Update(1, Fixture.TestCreatorId, new UpdateNoteTypeRequest 
            { 
                Name = "Update", 
                CssStyle = "", 
                Templates = [] 
            }));
    }
}
