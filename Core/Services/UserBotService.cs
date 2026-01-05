using Core.Data;
using Core.Dto.Common;
using Core.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace Core.Services;

public class UserBotService(DataContext context) : IUserBotService
{
    public async Task<ResponseResult<string>> Get(string botId)
    {
        string? userId = await context.UserBots
            .Where(x => x.BotId == botId)
            .Select(x => x.UserId)
            .FirstOrDefaultAsync();

        if (string.IsNullOrEmpty(userId))
        {
            return ResponseResult<string>.Failure(
                ErrorCode.NotFound,
                $"No user found associated with bot ID '{botId}'."
            );
        }

        return ResponseResult<string>.Success(userId);
    }

    public async Task<ResponseResult<bool>> Delete(string userId, int id)
    {
        int affectedRows = await context.UserBots
            .Where(x => x.Id == id && x.UserId == userId)
            .ExecuteDeleteAsync();

        if (affectedRows == 0)
        {
            return ResponseResult<bool>.Failure(
                ErrorCode.NotFound,
                $"Bot with ID {id} not found or you don't have permission to delete it."
            );
        }

        return ResponseResult<bool>.Success(true);
    }
}