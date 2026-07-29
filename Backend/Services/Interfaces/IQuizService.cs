using Backend.Common;
using Backend.DTOs;

namespace Backend.Services.Interfaces;

public interface IQuizService
{
    Task<ApiResponse<QuizAttemptResultDto>> SubmitAttemptAsync(int userId, int quizId, SubmitQuizAttemptDto dto, int? courseId = null);
    Task<ApiResponse<List<QuizAttemptDto>>> GetMyAttemptsAsync(int userId, int quizId);
}
