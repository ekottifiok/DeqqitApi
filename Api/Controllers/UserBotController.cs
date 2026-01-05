using Api.Services;
using Core.Dto.Common;
using Core.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserBotController(IUserBotService userBotService,  ICurrentUserService currentUserService): BaseController
{
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        string? userId = currentUserService.GetUserId();
        if (string.IsNullOrEmpty(userId)) return Forbid();

        ResponseResult<bool> result = await userBotService.Delete(userId, id);
        return ProcessResult(result);
    }
}