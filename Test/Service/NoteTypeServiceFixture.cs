using Core.Data;
using Core.Model;
using Core.Model.Faker;
using Test.Helper;

namespace Test.Service;

public class NoteTypeServiceFixture : DatabaseFixture
{
    public string TestCreatorId { get; } = "notetype-tester";

    protected override async Task SeedAsync(DataContext context)
    {
        User? user = new UserFaker("NoteTypeTester")
            .RuleFor(u => u.Id, _ => TestCreatorId)
            .Generate();
        context.Users.Add(user);
        await context.SaveChangesAsync();
    }
}
