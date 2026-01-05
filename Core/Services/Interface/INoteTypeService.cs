using Core.Dto.Common;
using Core.Dto.NoteType;
using Core.Model;

namespace Core.Services.Interface;

public interface INoteTypeService
{
    Task<PaginationResult<NoteType>> Get(string creatorId, PaginationRequest<int> request);

    Task<ResponseResult<NoteType>> Get(string creatorId, int id);

    Task<ResponseResult<NoteType>> Create(string creatorId, CreateNoteTypeRequest request);

    Task<ResponseResult<bool>> Update(int id, string creatorId, UpdateNoteTypeRequest request);

    Task<ResponseResult<bool>> Delete(int id, string creatorId);
}