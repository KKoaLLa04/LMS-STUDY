using Backend.Common;
using Backend.DTOs;

namespace Backend.Services.Interfaces;

public interface IQuizQuestionService
{
    Task<ApiResponse<List<QuizQuestionAdminDto>>> GetForAdminAsync(int quizId);

    // Không bao giờ trả về IsCorrect — dùng cho học sinh làm bài.
    Task<ApiResponse<List<QuizQuestionDto>>> GetForStudentAsync(int quizId);

    Task<ApiResponse<object?>> ReplaceQuestionsAsync(int quizId, ReplaceQuizQuestionsDto dto);
}
