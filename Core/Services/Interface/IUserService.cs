using Core.Dto.Common;
using Core.Dto.User;

namespace Core.Services.Interface;

public interface IUserService
{
    Task<ResponseResult<bool>> UpdateProfileImage(string id, string profileImage);
    Task<ResponseResult<UserResponse>> Get(string id);
    Task<ResponseResult<int?>> GetDefaultProviderId(string userId);
    Task<ResponseResult<bool>> Update(string id, UpdateUserRequest request);
    Task<ResponseResult<UserDashboardResponse>> GetUserDashboard(string userId);
    Task<ResponseResult<bool>> UpdateStreakDaily();
}
