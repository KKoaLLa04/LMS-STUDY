using Backend.Common;
using Backend.Data;
using Backend.DTOs;
using Backend.Models;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class QuizQuestionService : IQuizQuestionService
{
    private readonly AppDbContext _context;
    private readonly ILogger<QuizQuestionService> _logger;

    public QuizQuestionService(AppDbContext context, ILogger<QuizQuestionService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<List<QuizQuestionAdminDto>>> GetForAdminAsync(int quizId)
    {
        try
        {
            var questions = await _context.QuizQuestions
                .Where(q => q.QuizId == quizId)
                .OrderBy(q => q.OrderNumber)
                .Select(q => new QuizQuestionAdminDto
                {
                    Id = q.Id,
                    Text = q.Text,
                    OrderNumber = q.OrderNumber,
                    AllowMultipleAnswers = q.AllowMultipleAnswers,
                    Options = q.Options
                        .OrderBy(o => o.OrderNumber)
                        .Select(o => new QuizOptionAdminDto
                        {
                            Id = o.Id,
                            Text = o.Text,
                            IsCorrect = o.IsCorrect,
                            OrderNumber = o.OrderNumber
                        })
                        .ToList()
                })
                .ToListAsync();

            return ApiResponse<List<QuizQuestionAdminDto>>.Ok(questions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách câu hỏi quiz (admin) cho quiz {QuizId}", quizId);
            return ApiResponse<List<QuizQuestionAdminDto>>.Error("Đã xảy ra lỗi khi lấy danh sách câu hỏi");
        }
    }

    public async Task<ApiResponse<List<QuizQuestionDto>>> GetForStudentAsync(int quizId)
    {
        try
        {
            var questions = await _context.QuizQuestions
                .Where(q => q.QuizId == quizId)
                .OrderBy(q => q.OrderNumber)
                .Select(q => new QuizQuestionDto
                {
                    Id = q.Id,
                    Text = q.Text,
                    OrderNumber = q.OrderNumber,
                    AllowMultipleAnswers = q.AllowMultipleAnswers,
                    Options = q.Options
                        .OrderBy(o => o.OrderNumber)
                        .Select(o => new QuizOptionDto { Id = o.Id, Text = o.Text })
                        .ToList()
                })
                .ToListAsync();

            return ApiResponse<List<QuizQuestionDto>>.Ok(questions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách câu hỏi quiz cho quiz {QuizId}", quizId);
            return ApiResponse<List<QuizQuestionDto>>.Error("Đã xảy ra lỗi khi lấy danh sách câu hỏi");
        }
    }

    public async Task<ApiResponse<object?>> ReplaceQuestionsAsync(int quizId, ReplaceQuizQuestionsDto dto)
    {
        try
        {
            var quiz = await _context.Quizzes.FirstOrDefaultAsync(q => q.Id == quizId);
            if (quiz == null)
                return ApiResponse<object?>.NotFound("Không tìm thấy quiz");

            foreach (var q in dto.Questions)
            {
                if (q.Options.Count(o => o.IsCorrect) == 0)
                    return ApiResponse<object?>.BadRequest($"Câu hỏi \"{q.Text}\" cần ít nhất 1 đáp án đúng");
                if (!q.AllowMultipleAnswers && q.Options.Count(o => o.IsCorrect) > 1)
                    return ApiResponse<object?>.BadRequest($"Câu hỏi \"{q.Text}\" chỉ cho phép 1 đáp án đúng");
            }

            // Thay thế toàn bộ câu hỏi cũ bằng danh sách mới — đơn giản và an toàn vì chưa có
            // phân tích/thống kê nào gắn theo QuestionId/OptionId cụ thể (xem ghi chú DTO).
            var oldQuestions = await _context.QuizQuestions.Where(q => q.QuizId == quizId).ToListAsync();
            _context.QuizQuestions.RemoveRange(oldQuestions);

            var newQuestions = dto.Questions.Select(q => new QuizQuestion
            {
                QuizId = quizId,
                Text = q.Text.Trim(),
                OrderNumber = q.OrderNumber,
                AllowMultipleAnswers = q.AllowMultipleAnswers,
                Options = q.Options.Select(o => new QuizOption
                {
                    Text = o.Text.Trim(),
                    IsCorrect = o.IsCorrect,
                    OrderNumber = o.OrderNumber
                }).ToList()
            }).ToList();

            _context.QuizQuestions.AddRange(newQuestions);
            quiz.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return ApiResponse<object?>.Ok(null, "Lưu câu hỏi quiz thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lưu câu hỏi quiz cho quiz {QuizId}", quizId);
            return ApiResponse<object?>.Error("Đã xảy ra lỗi khi lưu câu hỏi quiz");
        }
    }
}
