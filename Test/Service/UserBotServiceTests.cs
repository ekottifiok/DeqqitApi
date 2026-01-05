using Core.Data;
using Core.Dto.Common;
using Core.Model;
using Core.Model.Helper;
using Core.Services;
using Microsoft.EntityFrameworkCore;
using Test.Helper;
using Xunit;

namespace Test.Service;

public class UserBotServiceTests(UserBotServiceFixture fixture) : IntegrationTestBase<UserBotServiceFixture>(fixture)
{
    private UserBotService _userBotService;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _userBotService = new UserBotService(Context);
    }

    [Fact]
    public async Task Get_ValidBotId_ReturnsUserId()
    {
        // Seed
        UserBot bot = new UserBot { BotId = "telegram-123", UserId = Fixture.TestCreatorId, Type = UserBotType.Telegram };
        Context.UserBots.Add(bot);
        await Context.SaveChangesAsync();

        ResponseResult<string> result = await _userBotService.Get("telegram-123");

        Assert.True(result.IsSuccess);
        Assert.Equal(Fixture.TestCreatorId, result.Value);
    }

    [Fact]
    public async Task Delete_ValidId_DeletesUserBot()
    {
        UserBot bot = new UserBot { BotId = "telegram-delete", UserId = Fixture.TestCreatorId, Type = UserBotType.Telegram };
        Context.UserBots.Add(bot);
        await Context.SaveChangesAsync();

        ResponseResult<bool> result = await _userBotService.Delete(Fixture.TestCreatorId, bot.Id);

        Assert.True(result.IsSuccess);
        
        Context.ChangeTracker.Clear();
        Assert.Null(await Context.UserBots.FindAsync(bot.Id));
    }
}
