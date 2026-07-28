using Backend.Data;
using Backend.Models;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class PointService : IPointService
{
    // Điểm/ngày và các mốc thưởng cho từng loại chuỗi ngày — xem phân tích nghiệp vụ:
    // đăng nhập liên tiếp và hoạt động học tập liên tiếp là hai chuỗi độc lập.
    private const int LoginStreakPointsPerDay = 2;
    private const int LoginStreakWeeklyBonus = 20;
    private const int LoginStreakMonthlyBonus = 100;

    private const int HomeworkStreakPointsPerDay = 5;
    private const int HomeworkStreakWeeklyBonus = 30;
    private const int HomeworkStreakMonthlyBonus = 150;

    private readonly AppDbContext _context;
    private readonly ILogger<PointService> _logger;

    public PointService(AppDbContext context, ILogger<PointService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task AwardAsync(int userId, int points, PointSourceType sourceType, int? sourceRefId = null, int? courseId = null)
    {
        if (points <= 0) return;

        _context.PointTransactions.Add(new PointTransaction
        {
            UserId = userId,
            Points = points,
            SourceType = sourceType,
            SourceRefId = sourceRefId,
            CourseId = courseId
        });

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi ghi giao dịch điểm cho user {UserId}, nguồn {SourceType}", userId, sourceType);
        }
    }

    public async Task RecordLoginStreakAsync(int userId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null) return;

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (user.LastLoginDate == today) return; // đã tính streak cho hôm nay rồi

        user.LoginStreakCount = user.LastLoginDate == today.AddDays(-1) ? user.LoginStreakCount + 1 : 1;
        user.LastLoginDate = today;
        await _context.SaveChangesAsync();

        var points = LoginStreakPointsPerDay;
        if (user.LoginStreakCount % 30 == 0) points += LoginStreakMonthlyBonus;
        else if (user.LoginStreakCount % 7 == 0) points += LoginStreakWeeklyBonus;

        await AwardAsync(userId, points, PointSourceType.LoginStreak, sourceRefId: user.LoginStreakCount);
    }

    public async Task RecordHomeworkStreakAsync(int userId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null) return;

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (user.LastActivityDate == today) return; // đã tính streak cho hôm nay rồi

        user.HomeworkStreakCount = user.LastActivityDate == today.AddDays(-1) ? user.HomeworkStreakCount + 1 : 1;
        user.LastActivityDate = today;
        await _context.SaveChangesAsync();

        var points = HomeworkStreakPointsPerDay;
        if (user.HomeworkStreakCount % 30 == 0) points += HomeworkStreakMonthlyBonus;
        else if (user.HomeworkStreakCount % 7 == 0) points += HomeworkStreakWeeklyBonus;

        await AwardAsync(userId, points, PointSourceType.HomeworkStreak, sourceRefId: user.HomeworkStreakCount);
    }
}
