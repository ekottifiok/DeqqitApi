using Core.Data;
using Core.Dto.Common;
using Core.Dto.NoteType;
using Core.Model;
using Core.Services.Helper.Interface;
using Core.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace Core.Services;

public class NoteTypeService(DataContext context, IHtmlService htmlService, ICssService cssService)
    : BaseService, INoteTypeService
{
    public async Task<PaginationResult<NoteType>> Get(string creatorId, PaginationRequest<int> request)
    {
        IQueryable<NoteType> query = context.NoteTypes.AsNoTracking()
            .Where(x => x.CreatorId == creatorId || x.CreatorId == null);

        return await PaginateAsync(query, request);
    }

    public async Task<NoteType?> Get(string creatorId, int id)
    {
        return await context.NoteTypes.AsNoTracking().Include(x => x.Templates)
            .FirstOrDefaultAsync(x => x.Id == id && (x.CreatorId == creatorId || x.CreatorId == null));
    }


    // public async Task<int> Update(int id, string creatorId, UpdateNoteTypeRequest request)
    // {
    //     NoteTypeRequest cleanupRequest = await Cleanup(request);
    //     // Note: Checking name existence might need to exclude the current 'id'
    //
    //     // 1. Fetch the parent to verify ownership
    //     var noteType = await context.NoteTypes
    //         .FirstOrDefaultAsync(x => x.Id == id && x.CreatorId == creatorId);
    //
    //     if (noteType == null) return 0;
    //
    //     // 2. Scalar updates on the parent
    //     noteType.Name = request.Name;
    //     noteType.CssStyle = request.CssStyle;
    //
    //     // 3. Prepare the templates for Synchronization
    //     var templatesToSync = request.Templates.Select(t => new NoteTypeTemplate
    //     {
    //         Id = t.Id,
    //         NoteTypeId = id, // Ensure FK is set
    //         Front = t.Front,
    //         Back = t.Back
    //     }).ToList();
    //
    //     // 4. Use BulkSynchronize for the child collection
    //     // This handles adding new, updating existing, AND deleting removed templates.
    //     await context.BulkSynchronizeAsync(templatesToSync, options => {
    //         options.ColumnPrimaryKeyExpression = t => t.Id;
    //         options.SynchronizeKeepidentity = true; // Keep existing if needed
    //     });
    //
    //     // 5. Save the scalar changes for the parent NoteType
    //     return await context.SaveChangesAsync();
    // }

    public async Task<NoteType?> Create(string creatorId, CreateNoteTypeRequest request)
    {
        NoteTypeRequest cleanupRequest = await Cleanup(request);
        if (await DoesNoteNameExist(cleanupRequest.Name)) return null;

        NoteType deck = new()
        {
            CreatorId = creatorId,
            Name = cleanupRequest.Name,
            Templates = cleanupRequest.Templates.Select(x => (NoteTypeTemplate)x).ToList(),
            CssStyle = cleanupRequest.CssStyle
        };
        await context.NoteTypes.AddAsync(deck);
        await context.SaveChangesAsync();
        return deck;
    }

    public Task<int> Update(int id, string creatorId, UpdateNoteTypeRequest request)
    {
        throw new NotImplementedException();
    }

    public async Task<int> Delete(int id, string creatorId)
    {
        return await context.NoteTypes
            .Where(x => x.CreatorId == creatorId && x.Id == id)
            .ExecuteDeleteAsync();
    }


    private async Task<bool> DoesNoteNameExist(string name)
    {
        return await context.NoteTypes.AnyAsync(x => x.Name == name && x.CreatorId == null);
    }

    private async Task<NoteTypeRequest> Cleanup(NoteTypeRequest request)
    {
        // Parsing all the HTML 
        IEnumerable<Task<(string? frontHtml, string? backHtml)>> tasks = request.Templates.Select(async template =>
            (await htmlService.Parse(template.Back), await htmlService.Parse(template.Front)));
        (string? frontHtml, string? backHtml)[] results = await Task.WhenAll(tasks);
        request.Templates = results
            .Where(r => r is { frontHtml: not null, backHtml: not null })
            .Select(r => new UpsertNoteTypeTemplateRequest { Back = r.backHtml!, Front = r.frontHtml! })
            .ToList();
        request.CssStyle = await cssService.Parse(request.CssStyle);
        return request;
    }
}