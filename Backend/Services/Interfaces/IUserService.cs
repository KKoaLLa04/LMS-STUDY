using Backend.Common;
using Backend.DTOs;
using Backend.Models;

namespace Backend.Services.Interfaces;

public interface IUserService
{
    Task<ApiResponse<List<UserDto>>> GetByRoleAsync(UserRole? role);
    Task<ApiResponse<UserDto>> GetByIdAsync(int id);
    Task<ApiResponse<UserDto>> CreateAsync(CreateUserDto dto);
    Task<ApiResponse<UserDto>> UpdateAsync(int id, UpdateUserDto dto);
    Task<ApiResponse<object?>> DeleteAsync(int id);
}
