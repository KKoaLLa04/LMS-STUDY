using Backend.Common;
using Backend.Data;
using Backend.DTOs;
using Backend.Models;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class QuizLibraryService : IQuizLibraryService
{
    private readonly AppDbContext _context;
    private readonly ILogger<QuizLibraryService> _logger;

    public QuizLibraryService(AppDbContext context, ILogger<QuizLibraryService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<List<QuizResponseDto>>> GetAllAsync()
    {
        try
        {
            var items = await _context.Quizzes
                .OrderByDescending(q => q.Id)
                .Select(q => new QuizResponseDto
                {
                    Id = q.Id,
                    Title = q.Title,
                    Description = q.Description,
                    QuestionCount = _context.QuizQuestions.Count(qq => qq.QuizId == q.Id),
                    CreatedAt = q.CreatedAt,
                    UpdatedAt = q.UpdatedAt,
                    LinkedLessonCount = _context.Lessons.Count(l => l.QuizId == q.Id)
                })
                .ToListAsync();

            return ApiResponse<List<QuizResponseDto>>.Ok(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách quiz chung");
            return ApiResponse<List<QuizResponseDto>>.Error("Đã xảy ra lỗi khi lấy danh sách quiz");
        }
    }

    public async Task<ApiResponse<QuizResponseDto>> GetByIdAsync(int id)
    {
        try
        {
            var quiz = await _context.Quizzes.FirstOrDefaultAsync(q => q.Id == id);
            if (quiz == null)
                return ApiResponse<QuizResponseDto>.NotFound("Không tìm thấy quiz");

            return ApiResponse<QuizResponseDto>.Ok(await MapToResponseDtoAsync(quiz));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy chi tiết quiz {Id}", id);
            return ApiResponse<QuizResponseDto>.Error("Đã xảy ra lỗi khi lấy thông tin quiz");
        }
    }

    public async Task<ApiResponse<QuizResponseDto>> CreateAsync(CreateQuizDto dto)
    {
        try
        {
            var quiz = new Quiz
            {
                Title = dto.Title.Trim(),
                Description = dto.Description?.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _context.Quizzes.Add(quiz);
            await _context.SaveChangesAsync();

            return ApiResponse<QuizResponseDto>.Ok(await MapToResponseDtoAsync(quiz), "Tạo quiz thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tạo quiz chung");
            return ApiResponse<QuizResponseDto>.Error("Đã xảy ra lỗi khi tạo quiz");
        }
    }

    public async Task<ApiResponse<QuizResponseDto>> UpdateAsync(int id, UpdateQuizDto dto)
    {
        try
        {
            var quiz = await _context.Quizzes.FirstOrDefaultAsync(q => q.Id == id);
            if (quiz == null)
                return ApiResponse<QuizResponseDto>.NotFound("Không tìm thấy quiz");

            quiz.Title = dto.Title.Trim();
            quiz.Description = dto.Description?.Trim();
            quiz.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return ApiResponse<QuizResponseDto>.Ok(await MapToResponseDtoAsync(quiz), "Cập nhật quiz thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi cập nhật quiz {Id}", id);
            return ApiResponse<QuizResponseDto>.Error("Đã xảy ra lỗi khi cập nhật quiz");
        }
    }

    public async Task<ApiResponse<object?>> DeleteAsync(int id)
    {
        try
        {
            var quiz = await _context.Quizzes.FirstOrDefaultAsync(q => q.Id == id);
            if (quiz == null)
                return ApiResponse<object?>.NotFound("Không tìm thấy quiz");

            var linkedCount = await _context.Lessons.CountAsync(l => l.QuizId == id);
            if (linkedCount > 0)
                return ApiResponse<object?>.BadRequest(
                    $"Không thể xóa — quiz đang được sử dụng ở {linkedCount} bài học. Vui lòng gỡ khỏi các bài học đó trước.");

            quiz.IsDeleted = true;
            quiz.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return ApiResponse<object?>.Ok(null, "Xóa quiz thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xóa quiz {Id}", id);
            return ApiResponse<object?>.Error("Đã xảy ra lỗi khi xóa quiz");
        }
    }

    public async Task<ApiResponse<List<StudentQuizDto>>> GetAllForStudentAsync()
    {
        try
        {
            var items = await _context.Quizzes
                .OrderByDescending(q => q.Id)
                .Select(q => new StudentQuizDto
                {
                    Id = q.Id,
                    Title = q.Title,
                    Description = q.Description,
                    QuestionCount = _context.QuizQuestions.Count(qq => qq.QuizId == q.Id)
                })
                .ToListAsync();

            return ApiResponse<List<StudentQuizDto>>.Ok(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách quiz chung cho học viên");
            return ApiResponse<List<StudentQuizDto>>.Error("Đã xảy ra lỗi khi lấy danh sách quiz");
        }
    }

    public async Task<ApiResponse<StudentQuizDto>> GetForStudentAsync(int id)
    {
        try
        {
            var quiz = await _context.Quizzes.FirstOrDefaultAsync(q => q.Id == id);
            if (quiz == null)
                return ApiResponse<StudentQuizDto>.NotFound("Không tìm thấy quiz");

            var questionCount = await _context.QuizQuestions.CountAsync(qq => qq.QuizId == id);
            return ApiResponse<StudentQuizDto>.Ok(new StudentQuizDto
            {
                Id = quiz.Id,
                Title = quiz.Title,
                Description = quiz.Description,
                QuestionCount = questionCount
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy quiz {Id} cho học viên", id);
            return ApiResponse<StudentQuizDto>.Error("Đã xảy ra lỗi khi lấy thông tin quiz");
        }
    }

    private async Task<QuizResponseDto> MapToResponseDtoAsync(Quiz q) => new()
    {
        Id = q.Id,
        Title = q.Title,
        Description = q.Description,
        QuestionCount = await _context.QuizQuestions.CountAsync(qq => qq.QuizId == q.Id),
        CreatedAt = q.CreatedAt,
        UpdatedAt = q.UpdatedAt,
        LinkedLessonCount = await _context.Lessons.CountAsync(l => l.QuizId == q.Id)
    };
}
