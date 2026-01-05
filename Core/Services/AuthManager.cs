using System.Diagnostics;
using Core.Data;
using Core.Dto.User;
using Core.Model;
using Core.Services.Helper;
using Core.Services.Helper.Interface;
using Core.Services.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Core.Services;

public class AuthManager(
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    DataContext context,
    ITimeService timeService,
    ITokenService tokenService) : IAuthManager
{
    public async Task<(User user, Dictionary<string, string[]>? errors)> Register(RegisterRequest request)
    {
        User user = new()
        {
            UserName = request.UserName,
            Email = request.Email
        };
        IdentityResult result = await userManager.CreateAsync(user, request.Password);
        return !result.Succeeded ? (user, CreateValidationProblem(result)) : (user, null);
    }

    public async Task<(User? user, string? signInState)> Login(LoginRequest request)
    {
        User? user = await userManager.FindByNameAsync(request.UserName);
        if (user == null)
            return (user, SignInResult.Failed.ToString());
        SignInResult attempt = await signInManager.PasswordSignInAsync(user, request.Password, false, true);
        return (user, !attempt.Succeeded ? attempt.ToString() : null);
    }

    public async Task<AuthResponse> GenerateToken(User user)
    {
        // Handling RefreshTokens
        UserRefreshToken refreshToken = new()
        {
            UserId = user.Id,
            Validity = timeService.UtcNow.AddDays(TokenService.RefreshTokenValidityInDays),
            Token = tokenService.GenerateRefreshToken()
        };
        await context.UserRefreshTokens.AddAsync(refreshToken);
        user.RefreshTokens.Add(refreshToken);
        await context.SaveChangesAsync();

        IList<string> roles = await userManager.GetRolesAsync(user);
        return new AuthResponse(user.Id, tokenService.CreateAccessToken(user, roles.ToList()),
            refreshToken.Token);
    }

    public async Task<User?> DeleteToken(string id, RefreshTokenRequest dto)
    {
        User? user = await context.Users.Include(x => x.RefreshTokens)
            .FirstOrDefaultAsync(user1 => user1.Id == id);
        UserRefreshToken? userRefreshToken = user?.RefreshTokens.FirstOrDefault(tokens =>
            tokens.Token == dto.RefreshToken && timeService.UtcNow <= tokens.Validity);
        if (userRefreshToken == null) return null;
        context.UserRefreshTokens.Remove(userRefreshToken);
        await context.SaveChangesAsync();
        return user;
    }

    public async Task<Dictionary<string, string[]>> ChangePassword(string id, ChangePasswordRequest request)
    {
        User? user = await userManager.FindByIdAsync(id);

        if (user is null)
            return CreateValidationProblem(IdentityResult.Failed(userManager.ErrorDescriber.InvalidToken()));
        IdentityResult result =
            await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        return !result.Succeeded ? CreateValidationProblem(result) : new Dictionary<string, string[]>();
    }

    public async Task<User?> DeleteAccount(string id)
    {
        User? user = await userManager.FindByIdAsync(id);
        if (user == null) return null;
        await userManager.DeleteAsync(user);
        return user;
    }

    private static Dictionary<string, string[]> CreateValidationProblem(IdentityResult result)
    {
        Debug.Assert(!result.Succeeded);
        Dictionary<string, string[]> errorDictionary = new(1);

        foreach (IdentityError error in result.Errors)
        {
            string[] newDescriptions;

            if (errorDictionary.TryGetValue(error.Code, out string[]? descriptions))
            {
                newDescriptions = new string[descriptions.Length + 1];
                Array.Copy(descriptions, newDescriptions, descriptions.Length);
                newDescriptions[descriptions.Length] = error.Description;
            }
            else
            {
                newDescriptions = [error.Description];
            }

            errorDictionary[error.Code] = newDescriptions;
        }

        return errorDictionary;
    }
}