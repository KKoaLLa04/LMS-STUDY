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

    public async Task<ApiResponse<LeaderboardResponseDto>> GetLeaderboardAsync(
        RankPeriod period, int? courseId, int? khoiHocId, int? currentUserId, int page, int pageSize)
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

            var students = await _context.Users
                .Where(u => studentIds.Contains(u.Id))
                .Select(u => new { u.Id, u.FullName, u.AvatarUrl, u.CreatedAt, u.KhoiHocId })
                .ToListAsync();

            if (khoiHocId.HasValue)
                students = students.Where(s => s.KhoiHocId == khoiHocId).ToList();

            var scopedStudentIds = students.Select(s => s.Id).ToList();

            var now = DateTime.UtcNow;
            // Kỳ trước = cùng độ dài kỳ hiện tại, liền kề về trước — so sánh hạng "kỳ này" với
            // "kỳ trước" mà không cần lưu snapshot: tính lại trực tiếp từ lịch sử PointTransactions.
            var (since, prevSince, prevUntil) = period switch
            {
                RankPeriod.Week => (now.AddDays(-7), now.AddDays(-14), now.AddDays(-7)),
                RankPeriod.Month => (now.AddMonths(-1), now.AddMonths(-2), now.AddMonths(-1)),
                _ => (DateTime.MinValue, DateTime.MinValue, now.AddDays(-7))
            };

            var currentTotals = await ComputeTotalsAsync(since, null, courseId, scopedStudentIds);
            var previousTotals = await ComputeTotalsAsync(prevSince, prevUntil, courseId, scopedStudentIds);

            var khoiHocNames = await _context.KhoiHocs.ToDictionaryAsync(k => k.Id, k => k.Name);

            // Người mà currentUser đang theo dõi — tính 1 lần cho cả trang thay vì N truy vấn/dòng.
            // Khách chưa đăng nhập (currentUserId null) thì bỏ qua, không ai được đánh dấu đang theo dõi.
            var followingIds = currentUserId.HasValue
                ? (await _context.Follows
                    .Where(f => f.FollowerId == currentUserId)
                    .Select(f => f.FollowingId)
                    .ToListAsync())
                    .ToHashSet()
                : new HashSet<int>();

            // Hạng kỳ trước: cùng cách xếp hạng (điểm cao hơn đứng trước, hòa thì tài khoản tạo sớm
            // hơn đứng trên) nhưng tính trên tổng điểm của cửa sổ thời gian trước đó.
            var previousRanks = students
                .Select(s => new { s.Id, s.CreatedAt, Total = previousTotals.GetValueOrDefault(s.Id, 0) })
                .OrderByDescending(x => x.Total)
                .ThenBy(x => x.CreatedAt)
                .Select((x, i) => new { x.Id, Rank = i + 1 })
                .ToDictionary(x => x.Id, x => x.Rank);

            // Xếp hạng kỳ hiện tại: tổng điểm cao hơn đứng trước; hòa điểm thì tài khoản tạo sớm hơn đứng trên.
            var ranked = students
                .Select(s => new
                {
                    s.Id,
                    s.FullName,
                    s.AvatarUrl,
                    s.CreatedAt,
                    s.KhoiHocId,
                    Total = currentTotals.GetValueOrDefault(s.Id, 0)
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
                    IsMe = x.Id == currentUserId,
                    IsFollowing = followingIds.Contains(x.Id),
                    KhoiHocId = x.KhoiHocId,
                    GradeName = x.KhoiHocId.HasValue ? khoiHocNames.GetValueOrDefault(x.KhoiHocId.Value) : null,
                    // Chỉ có ý nghĩa so sánh nếu đã từng có điểm ở kỳ trước — tránh hiển thị biến động
                    // giả cho học sinh mới hoàn toàn chưa có lịch sử điểm.
                    PreviousRank = previousTotals.ContainsKey(x.Id) ? previousRanks[x.Id] : null
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

    private async Task<Dictionary<int, int>> ComputeTotalsAsync(DateTime since, DateTime? until, int? courseId, List<int> studentIds)
    {
        var query = _context.PointTransactions.Where(t => t.CreatedAt >= since);
        if (until.HasValue)
            query = query.Where(t => t.CreatedAt < until.Value);
        if (courseId.HasValue)
            query = query.Where(t => t.CourseId == courseId);

        return await query
            .Where(t => studentIds.Contains(t.UserId))
            .GroupBy(t => t.UserId)
            .Select(g => new { UserId = g.Key, Total = g.Sum(t => t.Points) })
            .ToDictionaryAsync(x => x.UserId, x => x.Total);
    }
}
