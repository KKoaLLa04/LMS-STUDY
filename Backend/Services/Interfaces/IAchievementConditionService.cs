using Backend.Common;
using Backend.DTOs;

namespace Backend.Services.Interfaces;

public interface IAchievementConditionService
{
    Task<ApiResponse<List<AchievementConditionTypeDto>>> GetTypesAsync();
}
