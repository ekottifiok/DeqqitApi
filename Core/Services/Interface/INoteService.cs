using Core.Dto.Common;
using Core.Dto.Note;
using Core.Model;

namespace Core.Services.Interface;

public interface INoteService
{
    Task<PaginationResult<Note>> Get(string creatorId, int deckId, PaginationRequest<int> request);
    Task<Note?> Get(string creatorId, int id);

    Task<List<Dictionary<string, string>>?> GenerateFlashcards(string creatorId, int providerId, int noteTypeId,
        string description);

    Task<int> Update(string creatorId, int id, UpdateNoteRequest request);
    Task<bool> Create(string creatorId, int deckId, int noteTypeId, List<CreateNoteRequest> request);
    Task<int> Delete(int id, string creatorId);
}