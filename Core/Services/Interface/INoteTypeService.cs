using Core.Dto.Common;
using Core.Dto.NoteType;
using Core.Model;

namespace Core.Services.Interface;

public interface INoteTypeService
{
    Task<PaginationResult<NoteType>> Get(string creatorId, PaginationRequest<int> request);

    Task<NoteType?> Get(string creatorId, int id);

    Task<NoteType?> Create(string creatorId, CreateNoteTypeRequest request);

    Task<int> Update(int id, string creatorId, UpdateNoteTypeRequest request);

    Task<int> Delete(int id, string creatorId);
}