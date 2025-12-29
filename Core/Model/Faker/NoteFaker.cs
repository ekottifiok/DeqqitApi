using Bogus;
using Core.Model.Helper;

namespace Core.Model.Faker;

public sealed class NoteFaker : Faker<Note>
{
    public NoteFaker(int? deckId, int noteTypeId, string creatorId, bool shouldGenerateCard = true)
    {
        if (deckId is not null) RuleFor(n => n.DeckId, _ => deckId);
        RuleFor(n => n.NoteTypeId, _ => noteTypeId);
        RuleFor(n => n.CreatorId, f => creatorId);
        RuleFor(n => n.Data, f => new Dictionary<string, string>
        {
            ["Front"] = f.Lorem.Sentence(),
            ["Back"] = f.Lorem.Sentences(2),
            ["Example"] = f.Lorem.Paragraph()
        });
        RuleFor(n => n.Tags, f => f.Lorem.Words(f.Random.Int(0, 5)).ToList());
        if (shouldGenerateCard) RuleFor(n => n.Cards, GenerateCards);
    }

    private static ICollection<Card> GenerateCards(Bogus.Faker f)
    {
        List<Card> cards = [];
        int cardCount = f.Random.Int(1, 3); // Each note can have 1-3 cards
        (int Interval, int Repetitions, double EaseFactor, DateTime DueDate, int StepIndex) flashcardData =
            Card.FlashcardData();
        for (int i = 0; i < cardCount; i++)
            cards.Add(new Card
            {
                State = f.PickRandom<CardState>(),
                Interval = flashcardData.Interval,
                Repetitions = flashcardData.Repetitions,
                EaseFactor = flashcardData.EaseFactor,
                DueDate = f.Date.Future(),
                StepIndex = flashcardData.StepIndex,
                NoteTypeTemplateId = f.Random.Int(1, 2)
            });

        return cards;
    }
}