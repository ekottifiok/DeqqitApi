using System.Security.Cryptography;
using Core.Data;
using Core.Dto.Common;
using Core.Model;
using Core.Model.Helper;
using Core.Services.Helper.Interface;
using Core.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace Core.Services;

public class UserBotCodeService(DataContext context, ITimeService timeService) : IUserBotCodeService
{
    public async Task<ResponseResult<string>> GenerateCode(string userId)
    {   string code = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
        UserBotCode userBotCode = new()
        {
            RandomCode = code,
            UserId = userId,
            ExpirationDate = timeService.UtcNow.AddMinutes(5)
        };
        
        await context.UserBotCodes.AddAsync(userBotCode);
        await context.SaveChangesAsync();
        
        return ResponseResult<string>.Success(code);
    }

    public async Task<ResponseResult<bool>> VerifyCode(string code, string botId, UserBotType botType)
    {
        DateTime now = timeService.UtcNow;

        UserBotCode? botCode = await context.UserBotCodes
            .Where(x => x.RandomCode == code && now <= x.ExpirationDate)
            .FirstOrDefaultAsync();
            
        if (botCode is null)
        {
            return ResponseResult<bool>.Failure(
                ErrorCode.NotFound,
                "Invalid or expired verification code."
            );
        }

        bool doesProviderExist = await context.UserBots
            .AnyAsync(x => x.BotId == botId && x.UserId == botCode.UserId);
            
        if (doesProviderExist)
        {
            return ResponseResult<bool>.Success(true);
        }

        UserBot userBot = new()
        {
            BotId = botId,
            UserId = botCode.UserId,
            Type = botType
        };
        
        context.UserBots.Add(userBot);
        await context.SaveChangesAsync();
        
        return ResponseResult<bool>.Success(true);
    }
}