using Bogus;

namespace Core.Model.Faker;

public sealed class NoteTypeFaker : Faker<NoteType>
{
    public NoteTypeFaker(string creatorId)
    {
        RuleFor(nt => nt.Name, f => $"{f.Lorem.Word()} Note Type");
        RuleFor(nt => nt.CssStyle,
            f => $"color: {f.Internet.Color()}; font-family: {f.PickRandom("Arial", "Helvetica", "Times")}");
        RuleFor(nt => nt.CreatorId, f => creatorId);
        RuleFor(nt => nt.Templates, GenerateTemplates);
    }

    private static ICollection<NoteTypeTemplate> GenerateTemplates(Bogus.Faker f)
    {
        return new List<NoteTypeTemplate>
        {
            new()
            {
                Front = "{{Front}}",
                Back = "{{Back}}<br>{{Example}}"
            },
            new()
            {
                Front = "Question: {{Question}}",
                Back = "Answer: {{Answer}}"
            }
        };
    }
}