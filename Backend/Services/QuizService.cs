using Backend.Common;
using Backend.Data;
using Backend.DTOs;
using Backend.Models;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class QuizService : IQuizService
{
    private readonly AppDbContext _context;
    private readonly IPointService _pointService;
    private readonly ILogger<QuizService> _logger;

    public QuizService(AppDbContext context, IPointService pointService, ILogger<QuizService> logger)
    {
        _context = context;
        _pointService = pointService;
        _logger = logger;
    }

    public async Task<ApiResponse<QuizAttemptResultDto>> SubmitAttemptAsync(int userId, int lessonId, SubmitQuizAttemptDto dto)
    {
        try
        {
            var lesson = await _context.Lessons
                .Include(l => l.Section)
                .FirstOrDefaultAsync(l => l.Id == lessonId);

            if (lesson == null)
                return ApiResponse<QuizAttemptResultDto>.NotFound("Không tìm thấy bài học");

            if (lesson.LessonType != LessonType.Quiz)
                return ApiResponse<QuizAttemptResultDto>.BadRequest("Bài học này không phải dạng Quiz");

            var previousBest = await _context.QuizAttempts
                .Where(a => a.UserId == userId && a.LessonId == lessonId)
                .Select(a => (int?)a.ScorePercent)
                .MaxAsync() ?? 0;

            var attempt = new QuizAttempt
            {
                UserId = userId,
                LessonId = lessonId,
                ScorePercent = dto.ScorePercent
            };
            _context.QuizAttempts.Add(attempt);
            await _context.SaveChangesAsync();

            // Chỉ thưởng điểm cho phần cải thiện so với điểm cao nhất trước đó — học sinh có thể
            // làm lại quiz nhiều lần nhưng không "farm" điểm bằng cách nộp lại cùng một kết quả.
            var pointsAwarded = Math.Max(0, dto.ScorePercent - previousBest);
            if (pointsAwarded > 0)
                await _pointService.AwardAsync(userId, pointsAwarded, PointSourceType.QuizCompleted, lessonId, lesson.Section.CourseId);

            await _pointService.RecordHomeworkStreakAsync(userId);

            var result = new QuizAttemptResultDto
            {
                Attempt = MapToDto(attempt),
                BestScorePercent = Math.Max(previousBest, dto.ScorePercent),
                PointsAwarded = pointsAwarded
            };

            return ApiResponse<QuizAttemptResultDto>.Ok(result, "Nộp bài quiz thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi nộp bài quiz cho bài học {LessonId}, user {UserId}", lessonId, userId);
            return ApiResponse<QuizAttemptResultDto>.Error("Đã xảy ra lỗi khi nộp bài quiz");
        }
    }

    public async Task<ApiResponse<List<QuizAttemptDto>>> GetMyAttemptsAsync(int userId, int lessonId)
    {
        try
        {
            var items = await _context.QuizAttempts
                .Where(a => a.UserId == userId && a.LessonId == lessonId)
                .OrderByDescending(a => a.AttemptedAt)
                .Select(a => MapToDto(a))
                .ToListAsync();

            return ApiResponse<List<QuizAttemptDto>>.Ok(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy lịch sử làm quiz cho bài học {LessonId}, user {UserId}", lessonId, userId);
            return ApiResponse<List<QuizAttemptDto>>.Error("Đã xảy ra lỗi khi lấy lịch sử làm quiz");
        }
    }

    private static QuizAttemptDto MapToDto(QuizAttempt a) => new()
    {
        Id = a.Id,
        LessonId = a.LessonId,
        ScorePercent = a.ScorePercent,
        AttemptedAt = a.AttemptedAt
    };
}
