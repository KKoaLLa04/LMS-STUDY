using Backend.Common;
using Backend.DTOs;

namespace Backend.Services.Interfaces;

public interface IAchievementGroupService
{
    Task<ApiResponse<List<AchievementGroupDto>>> GetAllAsync();
    Task<ApiResponse<AchievementGroupDto>> CreateAsync(CreateAchievementGroupDto dto);
}
