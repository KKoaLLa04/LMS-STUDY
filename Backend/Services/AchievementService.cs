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
            var rows = await (
                from a in _context.Achievements
                join g in _context.AchievementGroups on a.GroupId equals g.Id into groupJoin
                from g in groupJoin.DefaultIfEmpty()
                orderby a.OrderNumber
                select new { Achievement = a, Group = g }
            ).ToListAsync();

            var conditionsByAchievementId = await _context.AchievementConditions
                .Where(c => rows.Select(r => r.Achievement.Id).Contains(c.AchievementId))
                .ToListAsync();

            var items = rows
                .Select(r => MapToDto(r.Achievement, r.Group, conditionsByAchievementId.Where(c => c.AchievementId == r.Achievement.Id)))
                .ToList();

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

            var group = await _context.AchievementGroups.FirstOrDefaultAsync(g => g.Id == achievement.GroupId);
            var conditions = await _context.AchievementConditions.Where(c => c.AchievementId == id).ToListAsync();
            return ApiResponse<AchievementDto>.Ok(MapToDto(achievement, group, conditions));
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
            var group = await _context.AchievementGroups.FirstOrDefaultAsync(g => g.Id == dto.GroupId);
            if (group == null)
                return ApiResponse<AchievementDto>.BadRequest("Nhóm huy hiệu không tồn tại");

            var achievement = new Achievement
            {
                Name = dto.Name.Trim(),
                Description = dto.Description.Trim(),
                GroupId = dto.GroupId,
                IconKey = dto.IconKey.Trim(),
                OrderNumber = dto.OrderNumber,
                Points = dto.Points
            };

            _context.Achievements.Add(achievement);
            await _context.SaveChangesAsync();

            var conditions = dto.Conditions.Select(c => new AchievementCondition
            {
                AchievementId = achievement.Id,
                ConditionType = c.ConditionType,
                TargetValue = c.TargetValue,
                LogicGroup = c.LogicGroup
            }).ToList();
            _context.AchievementConditions.AddRange(conditions);
            await _context.SaveChangesAsync();

            return ApiResponse<AchievementDto>.Ok(MapToDto(achievement, group, conditions), "Tạo huy hiệu thành tích thành công");
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

            var group = await _context.AchievementGroups.FirstOrDefaultAsync(g => g.Id == dto.GroupId);
            if (group == null)
                return ApiResponse<AchievementDto>.BadRequest("Nhóm huy hiệu không tồn tại");

            achievement.Name = dto.Name.Trim();
            achievement.Description = dto.Description.Trim();
            achievement.GroupId = dto.GroupId;
            achievement.IconKey = dto.IconKey.Trim();
            achievement.OrderNumber = dto.OrderNumber;
            achievement.Points = dto.Points;

            // Thay thế toàn bộ điều kiện cũ bằng danh sách mới — đơn giản hơn so với việc so khớp
            // (diff) từng điều kiện, và số lượng điều kiện mỗi huy hiệu luôn nhỏ.
            var existingConditions = await _context.AchievementConditions
                .Where(c => c.AchievementId == id)
                .ToListAsync();
            _context.AchievementConditions.RemoveRange(existingConditions);

            var conditions = dto.Conditions.Select(c => new AchievementCondition
            {
                AchievementId = id,
                ConditionType = c.ConditionType,
                TargetValue = c.TargetValue,
                LogicGroup = c.LogicGroup
            }).ToList();
            _context.AchievementConditions.AddRange(conditions);

            await _context.SaveChangesAsync();

            return ApiResponse<AchievementDto>.Ok(MapToDto(achievement, group, conditions), "Cập nhật huy hiệu thành tích thành công");
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

            var items = await (
                from a in _context.Achievements
                join g in _context.AchievementGroups on a.GroupId equals g.Id into groupJoin
                from g in groupJoin.DefaultIfEmpty()
                orderby a.OrderNumber
                select new MyAchievementDto
                {
                    Id = a.Id,
                    Name = a.Name,
                    Description = a.Description,
                    Category = g != null ? g.Code : string.Empty,
                    IconKey = a.IconKey,
                    OrderNumber = a.OrderNumber,
                    Points = a.Points
                }
            ).ToListAsync();

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

    private static AchievementDto MapToDto(Achievement achievement, AchievementGroup? group, IEnumerable<AchievementCondition> conditions) => new()
    {
        Id = achievement.Id,
        Name = achievement.Name,
        Description = achievement.Description,
        GroupId = achievement.GroupId,
        GroupName = group?.Name ?? string.Empty,
        Category = group?.Code ?? string.Empty,
        IconKey = achievement.IconKey,
        OrderNumber = achievement.OrderNumber,
        IsUnlocked = achievement.IsUnlocked,
        ProgressPercent = achievement.ProgressPercent,
        Points = achievement.Points,
        Conditions = conditions.Select(c => new AchievementConditionDto
        {
            Id = c.Id,
            ConditionType = c.ConditionType,
            TargetValue = c.TargetValue,
            LogicGroup = c.LogicGroup
        }).ToList()
    };
}
