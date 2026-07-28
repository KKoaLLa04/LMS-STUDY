using Backend.Common;
using Backend.Data;
using Backend.DTOs;
using Backend.Models;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class AchievementService : IAchievementService
{
    private readonly AppDbContext _context;
    private readonly IPointService _pointService;
    private readonly ILogger<AchievementService> _logger;

    public AchievementService(AppDbContext context, IPointService pointService, ILogger<AchievementService> logger)
    {
        _context = context;
        _pointService = pointService;
        _logger = logger;
    }

    public async Task<ApiResponse<List<AchievementDto>>> GetAllAsync()
    {
        try
        {
            var items = await _context.Achievements
                .OrderBy(a => a.OrderNumber)
                .Select(a => MapToDto(a))
                .ToListAsync();

            return ApiResponse<List<AchievementDto>>.Ok(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách huy hiệu thành tích");
            return ApiResponse<List<AchievementDto>>.Error("Đã xảy ra lỗi khi lấy danh sách huy hiệu thành tích");
        }
    }

    public async Task<ApiResponse<AchievementDto>> GetByIdAsync(int id)
    {
        try
        {
            var achievement = await _context.Achievements.FirstOrDefaultAsync(a => a.Id == id);
            if (achievement == null)
                return ApiResponse<AchievementDto>.NotFound("Huy hiệu thành tích không tồn tại");

            return ApiResponse<AchievementDto>.Ok(MapToDto(achievement));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy chi tiết huy hiệu thành tích {Id}", id);
            return ApiResponse<AchievementDto>.Error("Đã xảy ra lỗi khi lấy thông tin huy hiệu thành tích");
        }
    }

    public async Task<ApiResponse<AchievementDto>> CreateAsync(CreateAchievementDto dto)
    {
        try
        {
            var achievement = new Achievement
            {
                Name = dto.Name.Trim(),
                Description = dto.Description.Trim(),
                Category = dto.Category,
                IconKey = dto.IconKey.Trim(),
                OrderNumber = dto.OrderNumber,
                IsUnlocked = dto.IsUnlocked,
                ProgressPercent = dto.IsUnlocked ? 0 : dto.ProgressPercent,
                Points = dto.Points
            };

            _context.Achievements.Add(achievement);
            await _context.SaveChangesAsync();

            return ApiResponse<AchievementDto>.Ok(MapToDto(achievement), "Tạo huy hiệu thành tích thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tạo huy hiệu thành tích");
            return ApiResponse<AchievementDto>.Error("Đã xảy ra lỗi khi tạo huy hiệu thành tích");
        }
    }

    public async Task<ApiResponse<AchievementDto>> UpdateAsync(int id, UpdateAchievementDto dto)
    {
        try
        {
            var achievement = await _context.Achievements.FirstOrDefaultAsync(a => a.Id == id);
            if (achievement == null)
                return ApiResponse<AchievementDto>.NotFound("Huy hiệu thành tích không tồn tại");

            achievement.Name = dto.Name.Trim();
            achievement.Description = dto.Description.Trim();
            achievement.Category = dto.Category;
            achievement.IconKey = dto.IconKey.Trim();
            achievement.OrderNumber = dto.OrderNumber;
            achievement.IsUnlocked = dto.IsUnlocked;
            achievement.ProgressPercent = dto.IsUnlocked ? 0 : dto.ProgressPercent;
            achievement.Points = dto.Points;

            await _context.SaveChangesAsync();

            return ApiResponse<AchievementDto>.Ok(MapToDto(achievement), "Cập nhật huy hiệu thành tích thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi cập nhật huy hiệu thành tích {Id}", id);
            return ApiResponse<AchievementDto>.Error("Đã xảy ra lỗi khi cập nhật huy hiệu thành tích");
        }
    }

    public async Task<ApiResponse<object?>> DeleteAsync(int id)
    {
        try
        {
            var achievement = await _context.Achievements.FirstOrDefaultAsync(a => a.Id == id);
            if (achievement == null)
                return ApiResponse<object?>.NotFound("Huy hiệu thành tích không tồn tại");

            achievement.IsDeleted = true;
            achievement.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return ApiResponse<object?>.Ok(null, "Xóa huy hiệu thành tích thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xóa huy hiệu thành tích {Id}", id);
            return ApiResponse<object?>.Error("Đã xảy ra lỗi khi xóa huy hiệu thành tích");
        }
    }

    public async Task<ApiResponse<List<MyAchievementDto>>> GetMyAchievementsAsync(int userId)
    {
        try
        {
            var unlockedMap = await _context.UserAchievements
                .Where(ua => ua.UserId == userId)
                .ToDictionaryAsync(ua => ua.AchievementId, ua => ua.UnlockedAt);

            var items = await _context.Achievements
                .OrderBy(a => a.OrderNumber)
                .Select(a => new MyAchievementDto
                {
                    Id = a.Id,
                    Name = a.Name,
                    Description = a.Description,
                    Category = a.Category.ToString(),
                    IconKey = a.IconKey,
                    OrderNumber = a.OrderNumber,
                    Points = a.Points
                })
                .ToListAsync();

            foreach (var item in items)
            {
                if (!unlockedMap.TryGetValue(item.Id, out var unlockedAt)) continue;
                item.UnlockedByMe = true;
                item.UnlockedAt = unlockedAt;
            }

            return ApiResponse<List<MyAchievementDto>>.Ok(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách huy hiệu của user {UserId}", userId);
            return ApiResponse<List<MyAchievementDto>>.Error("Đã xảy ra lỗi khi lấy danh sách huy hiệu");
        }
    }

    public async Task<ApiResponse<object?>> UnlockForUserAsync(int userId, int achievementId)
    {
        try
        {
            var achievement = await _context.Achievements.FirstOrDefaultAsync(a => a.Id == achievementId);
            if (achievement == null)
                return ApiResponse<object?>.NotFound("Huy hiệu thành tích không tồn tại");

            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
                return ApiResponse<object?>.NotFound("Không tìm thấy học sinh");

            var alreadyUnlocked = await _context.UserAchievements
                .AnyAsync(ua => ua.UserId == userId && ua.AchievementId == achievementId);
            if (alreadyUnlocked)
                return ApiResponse<object?>.Ok(null, "Học sinh đã mở khóa huy hiệu này trước đó");

            _context.UserAchievements.Add(new UserAchievement { UserId = userId, AchievementId = achievementId });
            await _context.SaveChangesAsync();

            await _pointService.AwardAsync(userId, achievement.Points, PointSourceType.AchievementUnlocked, achievementId);

            return ApiResponse<object?>.Ok(null, "Mở khóa huy hiệu cho học sinh thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi mở khóa huy hiệu {AchievementId} cho user {UserId}", achievementId, userId);
            return ApiResponse<object?>.Error("Đã xảy ra lỗi khi mở khóa huy hiệu");
        }
    }

    private static AchievementDto MapToDto(Achievement achievement) => new()
    {
        Id = achievement.Id,
        Name = achievement.Name,
        Description = achievement.Description,
        Category = achievement.Category.ToString(),
        IconKey = achievement.IconKey,
        OrderNumber = achievement.OrderNumber,
        IsUnlocked = achievement.IsUnlocked,
        ProgressPercent = achievement.ProgressPercent,
        Points = achievement.Points
    };
}
