using Core.Data;
using Core.Dto.Common;
using Core.Model;
using Core.Model.Helper;
using Core.Services;
using Core.Services.Helper.Interface;
using Core.Services.Interface;
using Microsoft.EntityFrameworkCore;
using Moq;
using Test.Helper;
using Xunit;

namespace Test.Service;

public class UserBotCodeServiceTests(UserBotServiceFixture fixture) : IntegrationTestBase<UserBotServiceFixture>(fixture)
{
    private UserBotCodeService _codeService;
    private Mock<ITimeService> _mockTime;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        
        _mockTime = new Mock<ITimeService>();
        _mockTime.Setup(x => x.UtcNow).Returns(new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc));

        _codeService = new UserBotCodeService(Context, _mockTime.Object);
    }

    [Fact]
    public async Task GenerateCode_ValidUser_ReturnsCode()
    {
        ResponseResult<string> result = await _codeService.GenerateCode(Fixture.TestCreatorId);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(6, result.Value.Length);
        
        // Verify expiration
        UserBotCode? stored = await Context.UserBotCodes.FirstOrDefaultAsync(x => x.RandomCode == result.Value);
        Assert.NotNull(stored);
        Assert.Equal(Fixture.TestCreatorId, stored.UserId);
    }

    [Fact]
    public async Task VerifyCode_ValidCode_CreatesUserBot()
    {
        // Setup code
        string code = "123456";
        Context.UserBotCodes.Add(new UserBotCode 
        { 
            UserId = Fixture.TestCreatorId, 
            RandomCode = code, 
            ExpirationDate = _mockTime.Object.UtcNow.AddMinutes(5) 
        });
        await Context.SaveChangesAsync();

        ResponseResult<bool> result = await _codeService.VerifyCode(code, "bot-verified", UserBotType.Telegram);

        Assert.True(result.IsSuccess);
        
        Context.ChangeTracker.Clear();
        UserBot? bot = await Context.UserBots.FirstOrDefaultAsync(x => x.BotId == "bot-verified");
        Assert.NotNull(bot);
        Assert.Equal(Fixture.TestCreatorId, bot.UserId);
        Assert.Equal(UserBotType.Telegram, bot.Type);
    }

    [Fact]
    public async Task VerifyCode_ExpiredCode_ReturnsFailure()
    {
        // Setup expired code
        string code = "654321";
        Context.UserBotCodes.Add(new UserBotCode 
        { 
            UserId = Fixture.TestCreatorId, 
            RandomCode = code, 
            ExpirationDate = _mockTime.Object.UtcNow.AddMinutes(-1) 
        });
        await Context.SaveChangesAsync();

        ResponseResult<bool> result = await _codeService.VerifyCode(code, "bot-fail", UserBotType.Telegram);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.NotFound, result.Error?.Code);
    }
}
