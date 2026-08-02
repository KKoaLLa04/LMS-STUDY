using Backend.Common;
using Backend.DTOs;
using Backend.Models;

namespace Backend.Services.Interfaces;

public interface IUserService
{
    Task<ApiResponse<List<UserDto>>> GetByRoleAsync(UserRole? role);
    Task<ApiResponse<PagedResultDto<UserDto>>> GetPagedAsync(UserRole? role, int page, int pageSize, string? keyword);
    Task<ApiResponse<UserDto>> GetByIdAsync(int id);
    Task<ApiResponse<UserDto>> CreateAsync(CreateUserDto dto, bool callerIsAdmin);
    Task<ApiResponse<UserDto>> UpdateAsync(int id, UpdateUserDto dto, bool callerIsAdmin);
    Task<ApiResponse<object?>> DeleteAsync(int id);
    Task<ApiResponse<List<PublicUserDto>>> GetPublicTeachersAsync();
    Task<ApiResponse<PublicUserDto>> GetPublicByIdAsync(int id);
}
