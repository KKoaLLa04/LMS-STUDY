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
    private readonly IAchievementEvaluationService _achievementEvaluationService;
    private readonly ILogger<QuizService> _logger;

    public QuizService(
        AppDbContext context,
        IPointService pointService,
        IAchievementEvaluationService achievementEvaluationService,
        ILogger<QuizService> logger)
    {
        _context = context;
        _pointService = pointService;
        _achievementEvaluationService = achievementEvaluationService;
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
            var questionResults = BuildQuestionResults(questions, dto.Answers);
            var correctCount = questionResults.Count(r => r.IsCorrect);
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

            // Lưu lại chính xác đáp án đã chọn của lần làm này — để có thể khôi phục đúng màn
            // hình kết quả (tô đúng/sai từng câu) khi học sinh tải lại trang sau này.
            var answerOptions = questionResults
                .SelectMany(r => r.SelectedOptionIds.Select(optionId => new QuizAttemptAnswerOption
                {
                    QuizAttemptId = attempt.Id,
                    QuestionId = r.QuestionId,
                    OptionId = optionId
                }))
                .ToList();
            if (answerOptions.Count > 0)
            {
                _context.QuizAttemptAnswerOptions.AddRange(answerOptions);
                await _context.SaveChangesAsync();
            }

            // Chỉ thưởng điểm cho phần cải thiện so với điểm cao nhất trước đó — học sinh có thể
            // làm lại quiz nhiều lần nhưng không "farm" điểm bằng cách nộp lại cùng một kết quả.
            var pointsAwarded = Math.Max(0, scorePercent - previousBest);
            if (pointsAwarded > 0)
                await _pointService.AwardAsync(userId, pointsAwarded, PointSourceType.QuizCompleted, quizId, courseId);

            await _pointService.RecordHomeworkStreakAsync(userId);
            await _achievementEvaluationService.EvaluateAsync(userId);

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

    public async Task<ApiResponse<QuizAttemptResultDto>> GetAttemptDetailAsync(int userId, int attemptId)
    {
        try
        {
            var attempt = await _context.QuizAttempts.FirstOrDefaultAsync(a => a.Id == attemptId && a.UserId == userId);
            if (attempt == null)
                return ApiResponse<QuizAttemptResultDto>.NotFound("Không tìm thấy lần làm bài này");

            if (attempt.QuizId == null)
                return ApiResponse<QuizAttemptResultDto>.BadRequest("Lần làm bài này không còn xác định được quiz gốc");

            var questions = await _context.QuizQuestions
                .Include(q => q.Options)
                .Where(q => q.QuizId == attempt.QuizId)
                .ToListAsync();

            var storedAnswers = await _context.QuizAttemptAnswerOptions
                .Where(a => a.QuizAttemptId == attemptId)
                .ToListAsync();

            var answersByQuestion = storedAnswers
                .GroupBy(a => a.QuestionId)
                .Select(g => new QuizAnswerSubmissionDto
                {
                    QuestionId = g.Key,
                    SelectedOptionIds = g.Select(a => a.OptionId).ToList()
                })
                .ToList();

            var questionResults = BuildQuestionResults(questions, answersByQuestion);

            var bestScorePercent = await _context.QuizAttempts
                .Where(a => a.UserId == userId && a.QuizId == attempt.QuizId)
                .Select(a => (int?)a.ScorePercent)
                .MaxAsync() ?? attempt.ScorePercent;

            var result = new QuizAttemptResultDto
            {
                Attempt = MapToDto(attempt),
                BestScorePercent = bestScorePercent,
                PointsAwarded = 0,
                QuestionResults = questionResults
            };

            return ApiResponse<QuizAttemptResultDto>.Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy chi tiết lần làm quiz {AttemptId}, user {UserId}", attemptId, userId);
            return ApiResponse<QuizAttemptResultDto>.Error("Đã xảy ra lỗi khi lấy chi tiết lần làm quiz");
        }
    }

    // Dùng chung cho cả lúc chấm điểm khi nộp bài (answers từ request) lẫn lúc khôi phục lại
    // kết quả một lần làm cũ (answers dựng lại từ QuizAttemptAnswerOption đã lưu) — đảm bảo logic
    // đúng/sai luôn nhất quán giữa hai luồng.
    private static List<QuizQuestionResultDto> BuildQuestionResults(
        List<QuizQuestion> questions,
        List<QuizAnswerSubmissionDto> answers)
    {
        var results = new List<QuizQuestionResultDto>();
        foreach (var q in questions)
        {
            var correctOptionIds = q.Options.Where(o => o.IsCorrect).Select(o => o.Id).OrderBy(id => id).ToList();
            var submitted = answers.FirstOrDefault(a => a.QuestionId == q.Id);
            var selectedOptionIds = (submitted?.SelectedOptionIds ?? new List<int>()).OrderBy(id => id).ToList();
            // Không cho điểm một phần — phải chọn đúng chính xác tập đáp án đúng (áp dụng như
            // nhau cho câu 1 đáp án lẫn nhiều đáp án).
            var isCorrect = correctOptionIds.SequenceEqual(selectedOptionIds);

            results.Add(new QuizQuestionResultDto
            {
                QuestionId = q.Id,
                IsCorrect = isCorrect,
                CorrectOptionIds = correctOptionIds,
                SelectedOptionIds = selectedOptionIds
            });
        }
        return results;
    }

    private static QuizAttemptDto MapToDto(QuizAttempt a) => new()
    {
        Id = a.Id,
        QuizId = a.QuizId,
        ScorePercent = a.ScorePercent,
        AttemptedAt = a.AttemptedAt
    };
}
