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

    public async Task<ResponseResult<NoteType>> Get(string creatorId, int id)
    {
        NoteType? noteType = await context.NoteTypes.AsNoTracking()
            .Include(x => x.Templates)
            .FirstOrDefaultAsync(x => x.Id == id && (x.CreatorId == creatorId || x.CreatorId == null));
            
        if (noteType == null)
        {
            return ResponseResult<NoteType>.Failure(
                ErrorCode.NotFound,
                $"Note type with ID {id} not found or you don't have permission to access it."
            );
        }
        
        return ResponseResult<NoteType>.Success(noteType);
    }
    
    public async Task<ResponseResult<NoteType>> Create(string creatorId, CreateNoteTypeRequest request)
    {
        NoteTypeRequest cleanupRequest = await Cleanup(request);
        
        if (await DoesNoteNameExist(cleanupRequest.Name, creatorId))
        {
            return ResponseResult<NoteType>.Failure(
                ErrorCode.AlreadyExists,
                $"A note type with the name '{cleanupRequest.Name}' already exists."
            );
        }

        if (cleanupRequest.Templates.Count == 0)
        {
            return ResponseResult<NoteType>.Failure(
                ErrorCode.InvalidState,
                "Note type must have at least one template."
            );
        }

        NoteType noteType = new()
        {
            CreatorId = creatorId,
            Name = cleanupRequest.Name,
            Templates = cleanupRequest.Templates.Select(x => (NoteTypeTemplate)x).ToList(),
            CssStyle = cleanupRequest.CssStyle
        };
        
        await context.NoteTypes.AddAsync(noteType);
        await context.SaveChangesAsync();
        
        return ResponseResult<NoteType>.Success(noteType);
    }

    public Task<ResponseResult<bool>> Update(int id, string creatorId, UpdateNoteTypeRequest request)
    {
        throw new NotImplementedException();
    }

    public async Task<ResponseResult<bool>> Delete(int id, string creatorId)
    {
        int affectedRows = await context.NoteTypes
            .Where(x => x.CreatorId == creatorId && x.Id == id)
            .ExecuteDeleteAsync();
            
        if (affectedRows == 0)
        {
            return ResponseResult<bool>.Failure(
                ErrorCode.NotFound,
                $"Note type with ID {id} not found or you don't have permission to delete it."
            );
        }
        
        return ResponseResult<bool>.Success(true);
    }

    private async Task<bool> DoesNoteNameExist(string name, string creatorId)
    {
        return await context.NoteTypes.AnyAsync(x => x.Name == name && x.CreatorId == creatorId);
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