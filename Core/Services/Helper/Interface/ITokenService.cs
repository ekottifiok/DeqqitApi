using Microsoft.AspNetCore.Identity;

namespace Core.Services.Helper.Interface;

public interface ITokenService
{
    string GenerateRefreshToken();
    string CreateAccessToken(IdentityUser user, IEnumerable<string> userRoles);
}