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
    private readonly ILogger<AchievementService> _logger;

    public AchievementService(AppDbContext context, ILogger<AchievementService> logger)
    {
        _context = context;
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
                ProgressPercent = dto.IsUnlocked ? 0 : dto.ProgressPercent
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

    private static AchievementDto MapToDto(Achievement achievement) => new()
    {
        Id = achievement.Id,
        Name = achievement.Name,
        Description = achievement.Description,
        Category = achievement.Category.ToString(),
        IconKey = achievement.IconKey,
        OrderNumber = achievement.OrderNumber,
        IsUnlocked = achievement.IsUnlocked,
        ProgressPercent = achievement.ProgressPercent
    };
}
