using Core.Dto.Common;
using Core.Model.Helper;

namespace Core.Services.Interface;

public interface IUserBotCodeService
{
    Task<ResponseResult<string>> GenerateCode(string userId);
    Task<ResponseResult<bool>> VerifyCode(string code, string botId, UserBotType botType);
}
