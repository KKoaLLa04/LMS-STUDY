using Backend.Data;
using Backend.Models;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class AchievementEvaluationService : IAchievementEvaluationService
{
    private readonly AppDbContext _context;
    private readonly IPointService _pointService;
    private readonly ILogger<AchievementEvaluationService> _logger;

    public AchievementEvaluationService(AppDbContext context, IPointService pointService, ILogger<AchievementEvaluationService> logger)
    {
        _context = context;
        _pointService = pointService;
        _logger = logger;
    }

    public async Task EvaluateAsync(int userId)
    {
        try
        {
            var unlockedIds = await _context.UserAchievements
                .Where(ua => ua.UserId == userId)
                .Select(ua => ua.AchievementId)
                .ToListAsync();

            var candidates = await _context.Achievements
                .Where(a => !unlockedIds.Contains(a.Id))
                .Select(a => new { a.Id, a.Points })
                .ToListAsync();

            if (candidates.Count == 0) return;

            var candidateIds = candidates.Select(c => c.Id).ToList();
            var conditions = await _context.AchievementConditions
                .Where(c => candidateIds.Contains(c.AchievementId))
                .ToListAsync();

            if (conditions.Count == 0) return;

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return;

            // Cache giá trị hiện tại theo từng loại điều kiện trong phạm vi một lần gọi —
            // nhiều huy hiệu có thể dùng chung một loại điều kiện, không cần tính lại.
            var valueCache = new Dictionary<AchievementConditionType, int>();

            async Task<int> GetCurrentValueAsync(AchievementConditionType type)
            {
                if (valueCache.TryGetValue(type, out var cached)) return cached;
                var value = await ComputeCurrentValueAsync(userId, user, type);
                valueCache[type] = value;
                return value;
            }

            foreach (var achievementId in conditions.Select(c => c.AchievementId).Distinct())
            {
                var groups = conditions.Where(c => c.AchievementId == achievementId).GroupBy(c => c.LogicGroup);

                var satisfied = false;
                foreach (var group in groups)
                {
                    var allMet = true;
                    foreach (var condition in group)
                    {
                        var currentValue = await GetCurrentValueAsync(condition.ConditionType);
                        if (!IsSatisfied(condition.ConditionType, currentValue, condition.TargetValue))
                        {
                            allMet = false;
                            break;
                        }
                    }

                    if (allMet)
                    {
                        satisfied = true;
                        break;
                    }
                }

                if (!satisfied) continue;

                var alreadyUnlocked = await _context.UserAchievements
                    .AnyAsync(ua => ua.UserId == userId && ua.AchievementId == achievementId);
                if (alreadyUnlocked) continue;

                _context.UserAchievements.Add(new UserAchievement { UserId = userId, AchievementId = achievementId });
                await _context.SaveChangesAsync();

                var points = candidates.First(c => c.Id == achievementId).Points;
                await _pointService.AwardAsync(userId, points, PointSourceType.AchievementUnlocked, achievementId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi đánh giá điều kiện huy hiệu cho user {UserId}", userId);
        }
    }

    // Mặc định điều kiện đạt khi giá trị hiện tại >= ngưỡng, trừ RegisterBeforeHour — giờ đăng ký
    // càng nhỏ càng tốt (đăng ký trước giờ X trong ngày) nên so sánh theo chiều ngược lại.
    private static bool IsSatisfied(AchievementConditionType type, int currentValue, int targetValue) =>
        type == AchievementConditionType.RegisterBeforeHour
            ? currentValue < targetValue
            : currentValue >= targetValue;

    private async Task<int> ComputeCurrentValueAsync(int userId, User user, AchievementConditionType type)
    {
        switch (type)
        {
            case AchievementConditionType.CompleteLessonCount:
                return await _context.LessonProgresses.CountAsync(p => p.UserId == userId && p.IsCompleted);

            case AchievementConditionType.CompleteCourseCount:
                return await ComputeCompletedCourseCountAsync(userId);

            case AchievementConditionType.QuizPerfectScoreCount:
                return await _context.QuizAttempts
                    .Where(a => a.UserId == userId && a.ScorePercent == 100 && a.QuizId != null)
                    .Select(a => a.QuizId)
                    .Distinct()
                    .CountAsync();

            case AchievementConditionType.QuizMinScorePercent:
                return await _context.QuizAttempts
                    .Where(a => a.UserId == userId)
                    .Select(a => (int?)a.ScorePercent)
                    .MaxAsync() ?? 0;

            case AchievementConditionType.LoginStreakDays:
                return user.LoginStreakCount;

            case AchievementConditionType.CommentCount:
                return await _context.DiscussionPosts.CountAsync(p => p.UserId == userId);

            case AchievementConditionType.ReceivedLikeCount:
                return await _context.DiscussionPostLikes.CountAsync(l => l.Post.UserId == userId);

            case AchievementConditionType.FollowerCount:
                return await _context.Follows.CountAsync(f => f.FollowingId == userId);

            case AchievementConditionType.RegisterBeforeHour:
                return user.CreatedAt.Hour;

            default:
                return 0;
        }
    }

    // Khóa học được coi là hoàn thành khi mọi bài học thuộc khóa học đó (mà user đã ghi danh)
    // đều có LessonProgress.IsCompleted = true.
    private async Task<int> ComputeCompletedCourseCountAsync(int userId)
    {
        var enrolledCourseIds = await _context.Enrollments
            .Where(e => e.UserId == userId)
            .Select(e => e.CourseId)
            .ToListAsync();

        if (enrolledCourseIds.Count == 0) return 0;

        var totalLessonsByCourse = await _context.Lessons
            .Where(l => enrolledCourseIds.Contains(l.Section.CourseId))
            .GroupBy(l => l.Section.CourseId)
            .Select(g => new { CourseId = g.Key, Total = g.Count() })
            .ToListAsync();

        var completedLessonsByCourse = await _context.LessonProgresses
            .Where(p => p.UserId == userId && p.IsCompleted && enrolledCourseIds.Contains(p.Lesson.Section.CourseId))
            .GroupBy(p => p.Lesson.Section.CourseId)
            .Select(g => new { CourseId = g.Key, Completed = g.Count() })
            .ToListAsync();

        return totalLessonsByCourse.Count(t => t.Total > 0 &&
            completedLessonsByCourse.FirstOrDefault(c => c.CourseId == t.CourseId)?.Completed == t.Total);
    }
}
