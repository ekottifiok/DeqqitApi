using Core.Data;
using Core.Model;
using Core.Model.Faker;
using Core.Model.Helper;
using Test.Helper;

namespace Test.Service;

public class DeckServiceFixture : DatabaseFixture
{
    public string TestCreatorId { get; } = "deck-tester";
    public string OtherCreatorId { get; } = "other-tester";

    protected override async Task SeedAsync(DataContext context)
    {
        // 1. Create Test User
        User? user = new UserFaker("DeckTester")
            .RuleFor(u => u.Id, _ => TestCreatorId)
            .RuleFor(u => u.Email, _ => "deck@test.com")
            .RuleFor(u => u.DeckOption, _ => new DeckOption { NewLimitPerDay = 10, ReviewLimitPerDay = 50 })
            .Generate();

        // 2. Create Other User (for permission testing)
        User? otherUser = new UserFaker("OtherTester")
            .RuleFor(u => u.Id, _ => OtherCreatorId)
            .RuleFor(u => u.Email, _ => "other@test.com")
            .RuleFor(u => u.DeckOption, _ => new DeckOption { NewLimitPerDay = 10, ReviewLimitPerDay = 50 })
            .Generate();

        context.Users.AddRange(user, otherUser);
        await context.SaveChangesAsync();
    }
}
