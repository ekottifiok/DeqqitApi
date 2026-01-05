using Core.Dto.Common;

namespace Core.Services.Interface;

public interface IUserBotService
{
    Task<ResponseResult<string>> Get(string botId);
    Task<ResponseResult<bool>> Delete(string userId, int id);
}