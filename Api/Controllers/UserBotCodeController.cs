using Api.Services;
using Core.Dto.Common;
using Core.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserBotCodeController(IUserBotCodeService userBotCodeService, ICurrentUserService currentUserService): BaseController
{
    [HttpPost]
    public async Task<IActionResult> Create()
    {
        string? userId = currentUserService.GetUserId();
        if (string.IsNullOrEmpty(userId)) return Forbid();

        ResponseResult<string> result = await userBotCodeService.GenerateCode(userId);
        
        if (result.IsSuccess)
        {
            return Ok(new UserBotAuthCodeResponse(result.Value));
        }

        return ProcessResult(result);
    }

    public record UserBotAuthCodeResponse(string Code);
}