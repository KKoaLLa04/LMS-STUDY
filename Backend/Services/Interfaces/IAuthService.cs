using Backend.Common;
using Backend.DTOs;

namespace Backend.Services.Interfaces;

public interface IAuthService
{
    Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginDto dto);
    Task<ApiResponse<CurrentUserDto>> GetCurrentUserAsync(int userId);
    Task<ApiResponse<object?>> ChangePasswordAsync(int userId, ChangePasswordDto dto);
}
