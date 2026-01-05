using System.Security.Claims;
using Api.Helpers;
using Api.Services;
using Core.Dto.Common;
using Core.Dto.User;
using Core.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class UserController(IUserService userService, ICurrentUserService currentUserService): BaseController
{
    [HttpPut("profile-image")]
    public async Task<IActionResult> SetProfileImage(IFormFile file)
    {
        string? userId = currentUserService.GetUserId();
        if (string.IsNullOrEmpty(userId)) return Forbid();

        // Validation for file upload
        (bool success, string message, string? filePath) =
            await FileHelper.UploadFileAsync(file, FileHelper.FileImageUpload, FileHelper.FileImageExtensions);

        if (!success || filePath == null) return BadRequest(message);

        ResponseResult<bool> result = await userService.UpdateProfileImage(userId, filePath);
        
        // Custom wrap since we want to return the path if successful
        if (result.IsSuccess)
        {
             return Ok(new { Path = filePath });
        }
        return ProcessResult(result);
    }

    [HttpPut]
    public async Task<IActionResult> Update(UpdateUserRequest request)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Forbid();

        ResponseResult<bool> result = await userService.Update(userId, request);
        return ProcessResult(result);
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        string? userId = currentUserService.GetUserId();
        if (string.IsNullOrEmpty(userId)) return Forbid();

        ResponseResult<UserResponse> result = await userService.Get(userId);
        return ProcessResult(result);
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        string? userId = currentUserService.GetUserId();
        if (string.IsNullOrEmpty(userId)) return Forbid();

        ResponseResult<UserDashboardResponse> result = await userService.GetUserDashboard(userId);
        return ProcessResult(result);
    }
}