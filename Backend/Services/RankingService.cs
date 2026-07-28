using Backend.Common;
using Backend.Data;
using Backend.DTOs;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Backend.Services.Interfaces;

namespace Backend.Services;

public class RankingService : IRankingService
{
    private readonly AppDbContext _context;
    private readonly ILogger<RankingService> _logger;

    public RankingService(AppDbContext context, ILogger<RankingService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<LeaderboardResponseDto>> GetLeaderboardAsync(RankPeriod period, int? courseId, int currentUserId, int page, int pageSize)
    {
        try
        {
            // Phạm vi học sinh: toàn hệ thống (mọi Student) hoặc chỉ học sinh đã ghi danh khóa học này.
            List<int> studentIds;
            if (courseId.HasValue)
            {
                var courseExists = await _context.Courses.AnyAsync(c => c.Id == courseId);
                if (!courseExists)
                    return ApiResponse<LeaderboardResponseDto>.NotFound("Không tìm thấy khóa học");

                studentIds = await _context.Enrollments
                    .Where(e => e.CourseId == courseId)
                    .Select(e => e.UserId)
                    .ToListAsync();
            }
            else
            {
                studentIds = await _context.Users
                    .Where(u => u.Role == UserRole.Student)
                    .Select(u => u.Id)
                    .ToListAsync();
            }

            var since = period switch
            {
                RankPeriod.Week => DateTime.UtcNow.AddDays(-7),
                RankPeriod.Month => DateTime.UtcNow.AddMonths(-1),
                _ => DateTime.MinValue
            };

            var pointsQuery = _context.PointTransactions.Where(t => t.CreatedAt >= since);
            if (courseId.HasValue)
                pointsQuery = pointsQuery.Where(t => t.CourseId == courseId);

            var totals = await pointsQuery
                .Where(t => studentIds.Contains(t.UserId))
                .GroupBy(t => t.UserId)
                .Select(g => new { UserId = g.Key, Total = g.Sum(t => t.Points) })
                .ToDictionaryAsync(x => x.UserId, x => x.Total);

            var students = await _context.Users
                .Where(u => studentIds.Contains(u.Id))
                .Select(u => new { u.Id, u.FullName, u.AvatarUrl, u.CreatedAt })
                .ToListAsync();

            // Xếp hạng: tổng điểm cao hơn đứng trước; hòa điểm thì tài khoản tạo sớm hơn đứng trên.
            var ranked = students
                .Select(s => new
                {
                    s.Id,
                    s.FullName,
                    s.AvatarUrl,
                    s.CreatedAt,
                    Total = totals.GetValueOrDefault(s.Id, 0)
                })
                .OrderByDescending(x => x.Total)
                .ThenBy(x => x.CreatedAt)
                .Select((x, i) => new RankingEntryDto
                {
                    Rank = i + 1,
                    UserId = x.Id,
                    FullName = x.FullName,
                    AvatarUrl = x.AvatarUrl,
                    TotalPoints = x.Total,
                    IsMe = x.Id == currentUserId
                })
                .ToList();

            var totalCount = ranked.Count;
            var pageItems = ranked.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var response = new LeaderboardResponseDto
            {
                Items = new PagedResultDto<RankingEntryDto>
                {
                    Items = pageItems,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                },
                Me = ranked.FirstOrDefault(r => r.IsMe)
            };

            return ApiResponse<LeaderboardResponseDto>.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tính bảng xếp hạng (period={Period}, courseId={CourseId})", period, courseId);
            return ApiResponse<LeaderboardResponseDto>.Error("Đã xảy ra lỗi khi tính bảng xếp hạng");
        }
    }
}
