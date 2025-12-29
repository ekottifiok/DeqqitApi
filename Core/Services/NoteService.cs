using Core.Data;
using Core.Dto.Common;
using Core.Dto.Note;
using Core.Model;
using Core.Model.Helper;
using Core.Services.Helper;
using Core.Services.Helper.Interface;
using Core.Services.Interface;
using Microsoft.EntityFrameworkCore;
using Z.EntityFramework.Extensions;

namespace Core.Services;

public class NoteService(DataContext context, ITemplateService templateService) : BaseService, INoteService
{
    public async Task<PaginationResult<Note>> Get(string creatorId, int deckId, PaginationRequest<int> request)
    {
        IQueryable<Note> query = context.Notes.AsNoTracking()
            .Where(x => x.CreatorId == creatorId && x.DeckId == deckId);
        return await PaginateAsync(query, request);
    }

    public async Task<Note?> Get(string creatorId, int id)
    {
        return await context.Notes.AsNoTracking()
            .FirstOrDefaultAsync(x => x.CreatorId == creatorId && x.Id == id);
    }


    public async Task<int> Update(string creatorId, int id, UpdateNoteRequest request)
    {
        Note? note = await context.Notes
            .Include(x => x.NoteType)
            .ThenInclude(noteType => noteType.Templates)
            .FirstOrDefaultAsync(x => x.CreatorId == creatorId && x.Id == id);
        if (note == null) return 0;

        List<string> fields = templateService.GetAllFields(TemplateService.GetField(note.NoteType.Templates));
        note.Data = Cleanup(fields, request.Data);
        note.Tags = request.Tags;
        await context.SaveChangesAsync();
        return 1;
    }

    public async Task<List<Dictionary<string, string>>?> GenerateFlashcards(string creatorId, int providerId,
        int noteTypeId, string description)
    {
        List<UserAiProvider> providers = await context.Users.Include(x => x.AiProviders).Where(x => x.Id == creatorId)
            .AsNoTracking().Select(x => x.AiProviders)
            .FirstOrDefaultAsync() ?? [];
        providers.ForEach(x => Console.WriteLine($"ID: {x.Id}, Key: {x.Key}, Type: {x.Type}"));
        UserAiProvider? provider = providers.FirstOrDefault(x => x.Id == providerId);
        if (provider is null) return null;

        List<NoteTypeTemplate> templates =
            await context.NoteTypeTemplates.AsNoTracking().Where(x => x.NoteTypeId == noteTypeId).ToListAsync();
        IAiService? aiService = AiServiceFactory.GetUserService(provider);

        List<string> fields = templateService.GetAllFields(TemplateService.GetField(templates));
        List<Dictionary<string, string>>? flashcards = await aiService.GenerateFlashcard(fields, description);
        return flashcards ?? null;
    }

    public async Task<bool> Create(string creatorId, int deckId, int noteTypeId, List<CreateNoteRequest> request)
    {
        bool doesDeckExist = await context.Decks.AnyAsync(x => x.Id == deckId && x.CreatorId == creatorId);
        if (!doesDeckExist) return false;
        var noteType = await context.NoteTypes
            .Include(x => x.Templates)
            .Select(x => new { x.Id, x.CreatorId, x.Templates })
            .FirstOrDefaultAsync(x => x.Id == noteTypeId && (x.CreatorId == creatorId || x.CreatorId == null));
        if (noteType == null) return false;

        List<string> fields = templateService.GetAllFields(TemplateService.GetField(noteType.Templates));
        (int Interval, int Repetitions, double EaseFactor, DateTime DueDate, int StepIndex) flashcardData =
            Card.FlashcardData();
        List<Note> notes = request.Select(x => new Note
            {
                DeckId = deckId,
                NoteTypeId = noteTypeId,
                CreatorId = creatorId,
                Data = Cleanup(fields, x.Data),
                Tags = x.Tags,
                Cards = noteType.Templates.Select(template => new Card
                {
                    State = CardState.New,
                    Interval = flashcardData.Interval,
                    Repetitions = flashcardData.Repetitions,
                    EaseFactor = flashcardData.EaseFactor,
                    DueDate = flashcardData.DueDate,
                    StepIndex = flashcardData.StepIndex,
                    NoteTypeTemplateId = template.Id
                }).ToList()
            }
        ).Where(n => n.Data.Count > 0).ToList();
        BulkOptimizedAnalysis? analysis = await context.BulkInsertOptimizedAsync(notes,
            options => { options.IncludeGraph = true; });
        if (!analysis.IsOptimized) Console.WriteLine(analysis.TipsText); // View recommendations to optimize
        return true;
    }

    public async Task<int> Delete(int id, string creatorId)
    {
        return await context.Notes
            .Where(x => x.CreatorId == creatorId && x.Id == id)
            .ExecuteDeleteAsync();
    }


    private static Dictionary<string, string> Cleanup(List<string> fields,
        Dictionary<string, string> data)
    {
        return fields.ToDictionary(
            key => key,
            key => data.GetValueOrDefault(key, string.Empty)
        );
    }
}