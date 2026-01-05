using Core.Dto.Common;
using Core.Dto.Note;
using Core.Model;

namespace Core.Services.Interface;

public interface INoteService
{
    Task<PaginationResult<Note>> Get(string creatorId, int deckId, PaginationRequest<int> request);
    Task<ResponseResult<Note>> Get(string creatorId, int id);

    Task<ResponseResult<List<Dictionary<string, string>>>> GenerateFlashcards(string creatorId, int providerId, int noteTypeId,
        string description);

    Task<ResponseResult<bool>> Update(string creatorId, int id, UpdateNoteRequest request);
    Task<ResponseResult<bool>> Create(string creatorId, int deckId, int noteTypeId, List<CreateNoteRequest> request);
    Task<ResponseResult<bool>> Delete(int id, string creatorId);
}