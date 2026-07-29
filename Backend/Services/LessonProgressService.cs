using Backend.Common;
using Backend.Data;
using Backend.DTOs;
using Backend.Models;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class LessonProgressService : ILessonProgressService
{
    // Điểm cho mỗi 10 phút xem thực tế, và điểm thưởng khi hoàn thành bài học (xem >= 90% thời lượng).
    private const int SecondsPerPoint = 600;
    private const int LessonCompletedBonus = 10;
    private const double CompletionThreshold = 0.9;

    private readonly AppDbContext _context;
    private readonly IPointService _pointService;
    private readonly IAchievementEvaluationService _achievementEvaluationService;
    private readonly ILogger<LessonProgressService> _logger;

    public LessonProgressService(
        AppDbContext context,
        IPointService pointService,
        IAchievementEvaluationService achievementEvaluationService,
        ILogger<LessonProgressService> logger)
    {
        _context = context;
        _pointService = pointService;
        _achievementEvaluationService = achievementEvaluationService;
        _logger = logger;
    }

    public async Task<ApiResponse<LessonProgressDto>> UpdateProgressAsync(int userId, int lessonId, UpdateLessonProgressDto dto)
    {
        try
        {
            var lesson = await _context.Lessons
                .Include(l => l.Section).ThenInclude(s => s.Course)
                .FirstOrDefaultAsync(l => l.Id == lessonId);

            if (lesson == null)
                return ApiResponse<LessonProgressDto>.NotFound("Không tìm thấy bài học");

            var progress = await _context.LessonProgresses
                .FirstOrDefaultAsync(p => p.UserId == userId && p.LessonId == lessonId);

            if (progress == null)
            {
                progress = new LessonProgress { UserId = userId, LessonId = lessonId };
                _context.LessonProgresses.Add(progress);
            }

            var courseId = lesson.Section.CourseId;
            var durationSeconds = lesson.DurationMinutes * 60;
            var oldWatched = progress.WatchedSeconds;
            // Cao điểm không giảm — bỏ qua việc tua lùi để không mất điểm đã tích lũy,
            // đồng thời chặn farm điểm bằng cách gửi lại tiến độ thấp rồi tăng dần lại.
            var newWatched = Math.Max(oldWatched, dto.WatchedSeconds);

            if (durationSeconds > 0)
            {
                var cappedOld = Math.Min(oldWatched, durationSeconds);
                var cappedNew = Math.Min(newWatched, durationSeconds);
                var deltaPoints = (cappedNew / SecondsPerPoint) - (cappedOld / SecondsPerPoint);
                if (deltaPoints > 0)
                    await _pointService.AwardAsync(userId, deltaPoints, PointSourceType.VideoWatched, lessonId, courseId);

                if (!progress.IsCompleted && cappedNew >= durationSeconds * CompletionThreshold)
                {
                    progress.IsCompleted = true;
                    progress.CompletedAt = DateTime.UtcNow;
                    await _pointService.AwardAsync(userId, LessonCompletedBonus, PointSourceType.LessonCompleted, lessonId, courseId);
                }
            }

            progress.WatchedSeconds = newWatched;
            progress.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await _pointService.RecordHomeworkStreakAsync(userId);
            await _achievementEvaluationService.EvaluateAsync(userId);

            return ApiResponse<LessonProgressDto>.Ok(MapToDto(progress, lesson.Title));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi cập nhật tiến độ bài học {LessonId} cho user {UserId}", lessonId, userId);
            return ApiResponse<LessonProgressDto>.Error("Đã xảy ra lỗi khi cập nhật tiến độ bài học");
        }
    }

    public async Task<ApiResponse<List<LessonProgressDto>>> GetMyProgressForCourseAsync(int userId, int courseId)
    {
        try
        {
            var items = await _context.LessonProgresses
                .Include(p => p.Lesson)
                .Where(p => p.UserId == userId && p.Lesson.Section.CourseId == courseId)
                .Select(p => new LessonProgressDto
                {
                    LessonId = p.LessonId,
                    LessonTitle = p.Lesson.Title,
                    WatchedSeconds = p.WatchedSeconds,
                    IsCompleted = p.IsCompleted,
                    CompletedAt = p.CompletedAt
                })
                .ToListAsync();

            return ApiResponse<List<LessonProgressDto>>.Ok(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy tiến độ học tập của user {UserId} cho khóa học {CourseId}", userId, courseId);
            return ApiResponse<List<LessonProgressDto>>.Error("Đã xảy ra lỗi khi lấy tiến độ học tập");
        }
    }

    private static LessonProgressDto MapToDto(LessonProgress p, string lessonTitle) => new()
    {
        LessonId = p.LessonId,
        LessonTitle = lessonTitle,
        WatchedSeconds = p.WatchedSeconds,
        IsCompleted = p.IsCompleted,
        CompletedAt = p.CompletedAt
    };
}
