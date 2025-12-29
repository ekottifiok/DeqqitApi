using Bogus;
using Core.Model.Helper;

namespace Core.Model.Faker;

public sealed class DeckFaker : Faker<Deck>
{
    public DeckFaker(string creatorId)
    {
        RuleFor(d => d.Name, f => f.Lorem.Word());
        RuleFor(d => d.Description, f => f.Lorem.Sentence());
        RuleFor(d => d.CreatorId, _ => creatorId);
        RuleFor(u => u.Option, _ => new DeckOptionFaker());
        RuleFor(d => d.Notes, _ => new List<Note>());
        RuleFor(d => d.Cards, _ => new List<Card>());
        RuleFor(d => d.DailyCounts, GenerateDailyCounts);
    }

    private static ICollection<DeckDailyCount> GenerateDailyCounts(Bogus.Faker f)
    {
        List<DeckDailyCount> counts = [];
        int days = f.Random.Int(0, 7);

        for (int i = 0; i < days; i++)
            counts.Add(new DeckDailyCount
            {
                Date = DateOnly.FromDateTime(f.Date.Recent(i)),
                CardState = f.PickRandom<CardState>(),
                Count = f.Random.Int(1, 50)
            });

        return counts;
    }
}