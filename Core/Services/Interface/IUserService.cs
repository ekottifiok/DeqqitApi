using Core.Dto.User;
using Core.Model;

namespace Core.Services.Interface;

public interface IUserService
{
    Task<UserDashboardResponse?> GetUserDashboard(string userId);
    Task<int> UpdateProfileImage(string id, string profileImage);
    Task<UserResponse?> Get(string id);
    Task<int> Update(string id, UpdateUserRequest request);
}