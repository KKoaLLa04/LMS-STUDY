using Backend.Common;
using Backend.DTOs;

namespace Backend.Services.Interfaces;

public interface IPermissionGroupService
{
    Task<ApiResponse<List<PermissionGroupDto>>> GetAllAsync();
    Task<ApiResponse<PermissionGroupDetailDto>> GetByIdAsync(int id);
    Task<ApiResponse<PermissionGroupDetailDto>> CreateAsync(CreatePermissionGroupDto dto);
    Task<ApiResponse<PermissionGroupDetailDto>> UpdateAsync(int id, UpdatePermissionGroupDto dto);
    Task<ApiResponse<object?>> DeleteAsync(int id);
    Task<ApiResponse<object?>> SetMembersAsync(int groupId, List<int> teacherUserIds);
}
