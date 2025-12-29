using System.Security.Claims;
using Core.Dto.User;
using Core.Model;
using Core.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthManager authManager) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        (User _, Dictionary<string, string[]>? errors) = await authManager.Register(request);

        return errors != null ? ValidationProblem(new ValidationProblemDetails(errors)) : Created();
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        (User? user, string? signInState) = await authManager.Login(request);

        if (signInState != null || user == null)
            return Problem(signInState, statusCode: StatusCodes.Status401Unauthorized);

        AuthResponse response = await authManager.GenerateToken(user);
        return Ok(response);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh([FromBody] RefreshTokenRequest request)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Forbid();

        User? user = await authManager.DeleteToken(userId, request);

        if (user == null) return Challenge();

        AuthResponse authResponse = await authManager.GenerateToken(user);
        return Ok(authResponse);
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Forbid();

        await authManager.ChangePassword(userId, request);
        return NoContent();
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Forbid();

        await authManager.DeleteToken(userId, request);
        return NoContent();
    }
}