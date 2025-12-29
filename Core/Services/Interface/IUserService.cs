using Core.Dto.User;

namespace Core.Services.Interface;

public interface IUserService
{
    Task<UserDashboardResponse?> GetUserDashboard(string userId);
    Task<int> UpdateProfileImage(string id, string profileImage);
    Task<UserResponse?> Get(string id);
    Task<int> Update(string id, UpdateUserRequest request);
    Task UpdateStreakDaily();
}