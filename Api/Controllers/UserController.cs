using System.Security.Claims;
using Api.Helpers;
using Core.Dto.User;
using Core.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class UserController(IUserService userService) : ControllerBase
{
    [HttpPut("profile-image")]
    public async Task<IActionResult> SetProfileImage(IFormFile file)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Forbid();

        // Validation for file upload
        (bool success, string message, string? filePath) =
            await FileHelper.UploadFileAsync(file, FileHelper.FileImageUpload, FileHelper.FileImageExtensions);

        if (!success || filePath == null) return BadRequest(message);

        int updatedCount = await userService.UpdateProfileImage(userId, filePath);
        if (updatedCount == 0) return NotFound();

        return Ok(new { Path = filePath });
    }

    [HttpPut]
    public async Task<IActionResult> Update(UpdateUserRequest request)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Forbid();

        int updatedCount = await userService.Update(userId, request);
        if (updatedCount == 0) return NotFound();

        return NoContent();
    }

    [HttpGet]
    public async Task<ActionResult<UserResponse>> Get()
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Forbid();

        UserResponse? user = await userService.Get(userId);
        if (user is null) return Forbid();

        return Ok(user);
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<UserDashboardResponse>> GetDashboard()
    {
        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Forbid();

        UserDashboardResponse? userDashboard = await userService.GetUserDashboard(userId);
        if (userDashboard == null) return Forbid();

        return Ok(userDashboard);
    }
}