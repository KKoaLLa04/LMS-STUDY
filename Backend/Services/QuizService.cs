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

    public async Task<ApiResponse<QuizAttemptResultDto>> SubmitAttemptAsync(int userId, int quizId, SubmitQuizAttemptDto dto, int? courseId = null)
    {
        try
        {
            var quizExists = await _context.Quizzes.AnyAsync(q => q.Id == quizId);
            if (!quizExists)
                return ApiResponse<QuizAttemptResultDto>.NotFound("Không tìm thấy quiz");

            var questions = await _context.QuizQuestions
                .Include(q => q.Options)
                .Where(q => q.QuizId == quizId)
                .ToListAsync();

            if (questions.Count == 0)
                return ApiResponse<QuizAttemptResultDto>.BadRequest("Quiz này chưa có câu hỏi");

            // Chấm điểm hoàn toàn ở server — client chỉ gửi lên các option đã chọn, không được
            // phép tự khai báo điểm số (tránh học sinh sửa request để tự cho điểm tối đa).
            var questionResults = new List<QuizQuestionResultDto>();
            var correctCount = 0;

            foreach (var q in questions)
            {
                var correctOptionIds = q.Options.Where(o => o.IsCorrect).Select(o => o.Id).OrderBy(id => id).ToList();
                var submitted = dto.Answers.FirstOrDefault(a => a.QuestionId == q.Id);
                var selectedOptionIds = (submitted?.SelectedOptionIds ?? new List<int>()).OrderBy(id => id).ToList();
                // Không cho điểm một phần — phải chọn đúng chính xác tập đáp án đúng (áp dụng
                // như nhau cho câu 1 đáp án lẫn nhiều đáp án).
                var isCorrect = correctOptionIds.SequenceEqual(selectedOptionIds);
                if (isCorrect) correctCount++;

                questionResults.Add(new QuizQuestionResultDto
                {
                    QuestionId = q.Id,
                    IsCorrect = isCorrect,
                    CorrectOptionIds = correctOptionIds,
                    SelectedOptionIds = selectedOptionIds
                });
            }

            var scorePercent = (int)Math.Round(correctCount * 100.0 / questions.Count);

            var previousBest = await _context.QuizAttempts
                .Where(a => a.UserId == userId && a.QuizId == quizId)
                .Select(a => (int?)a.ScorePercent)
                .MaxAsync() ?? 0;

            var attempt = new QuizAttempt
            {
                UserId = userId,
                QuizId = quizId,
                ScorePercent = scorePercent
            };
            _context.QuizAttempts.Add(attempt);
            await _context.SaveChangesAsync();

            // Chỉ thưởng điểm cho phần cải thiện so với điểm cao nhất trước đó — học sinh có thể
            // làm lại quiz nhiều lần nhưng không "farm" điểm bằng cách nộp lại cùng một kết quả.
            var pointsAwarded = Math.Max(0, scorePercent - previousBest);
            if (pointsAwarded > 0)
                await _pointService.AwardAsync(userId, pointsAwarded, PointSourceType.QuizCompleted, quizId, courseId);

            await _pointService.RecordHomeworkStreakAsync(userId);

            var result = new QuizAttemptResultDto
            {
                Attempt = MapToDto(attempt),
                BestScorePercent = Math.Max(previousBest, scorePercent),
                PointsAwarded = pointsAwarded,
                QuestionResults = questionResults
            };

            return ApiResponse<QuizAttemptResultDto>.Ok(result, "Nộp bài quiz thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi nộp bài quiz {QuizId}, user {UserId}", quizId, userId);
            return ApiResponse<QuizAttemptResultDto>.Error("Đã xảy ra lỗi khi nộp bài quiz");
        }
    }

    public async Task<ApiResponse<List<QuizAttemptDto>>> GetMyAttemptsAsync(int userId, int quizId)
    {
        try
        {
            var items = await _context.QuizAttempts
                .Where(a => a.UserId == userId && a.QuizId == quizId)
                .OrderByDescending(a => a.AttemptedAt)
                .Select(a => MapToDto(a))
                .ToListAsync();

            return ApiResponse<List<QuizAttemptDto>>.Ok(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy lịch sử làm quiz {QuizId}, user {UserId}", quizId, userId);
            return ApiResponse<List<QuizAttemptDto>>.Error("Đã xảy ra lỗi khi lấy lịch sử làm quiz");
        }
    }

    private static QuizAttemptDto MapToDto(QuizAttempt a) => new()
    {
        Id = a.Id,
        QuizId = a.QuizId,
        ScorePercent = a.ScorePercent,
        AttemptedAt = a.AttemptedAt
    };
}
