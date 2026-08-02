using Backend.Common;
using Backend.DTOs;

namespace Backend.Services.Interfaces;

public interface IAchievementService
{
    Task<ApiResponse<List<AchievementDto>>> GetAllAsync();
    Task<ApiResponse<AchievementDto>> GetByIdAsync(int id);
    Task<ApiResponse<AchievementDto>> CreateAsync(CreateAchievementDto dto);
    Task<ApiResponse<AchievementDto>> UpdateAsync(int id, UpdateAchievementDto dto);
    Task<ApiResponse<object?>> DeleteAsync(int id);

    // userId == null => khách chưa đăng nhập xem catalogue công khai, mọi huy hiệu hiển thị ở trạng thái chưa mở khóa.
    Task<ApiResponse<List<MyAchievementDto>>> GetMyAchievementsAsync(int? userId);
    Task<ApiResponse<object?>> UnlockForUserAsync(int userId, int achievementId);
}
