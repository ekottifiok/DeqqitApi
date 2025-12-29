using Bogus;
using Core.Model.Helper;

namespace Core.Model.Faker;

public sealed class CardFaker : Faker<Card>
{
    public CardFaker(int? noteId, int noteTypeTemplateId, CardState? state = null, DateTime? dueDate = null)
    {
        if (noteId is not null) RuleFor(c => c.NoteId, _ => noteId);
        RuleFor(c => c.NoteTypeTemplateId, _ => noteTypeTemplateId);
        RuleFor(c => c.State, f => state ?? f.PickRandom<CardState>());
        RuleFor(c => c.Interval, f => f.Random.Int(0, 365));
        RuleFor(c => c.Repetitions, f => f.Random.Int(0, 50));
        RuleFor(c => c.EaseFactor, f => f.Random.Double(1.3, 3.0));
        RuleFor(c => c.DueDate, f => dueDate ?? f.Date.Recent(5).ToUniversalTime());
        RuleFor(c => c.StepIndex, f => f.Random.Int(0, 1));
    }
}