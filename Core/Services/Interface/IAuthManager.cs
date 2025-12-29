using Core.Dto.User;
using Core.Model;

namespace Core.Services.Interface;

public interface IAuthManager
{
    Task<(User user, Dictionary<string, string[]>? errors)> Register(RegisterRequest request);

    Task<(User? user, string? signInState)> Login(LoginRequest request);

    Task<AuthResponse> GenerateToken(User user);

    Task<User?> DeleteToken(string id, RefreshTokenRequest dto);

    Task<Dictionary<string, string[]>> ChangePassword(string id, ChangePasswordRequest request);

    Task<User?> DeleteAccount(string id);
}