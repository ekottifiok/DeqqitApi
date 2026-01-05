using Core.Data;
using Core.Model;
using Core.Model.Faker;
using Test.Helper;

namespace Test.Service;

public class UserBotServiceFixture : DatabaseFixture
{
    public string TestCreatorId { get; } = "bot-tester";

    protected override async Task SeedAsync(DataContext context)
    {
        User? user = new UserFaker("BotTester")
            .RuleFor(u => u.Id, _ => TestCreatorId)
            .Generate();
        context.Users.Add(user);
        await context.SaveChangesAsync();
    }
}
