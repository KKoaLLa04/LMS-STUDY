using Backend.Common;
using Backend.DTOs;

namespace Backend.Services.Interfaces;

public interface IQuizService
{
    Task<ApiResponse<QuizAttemptResultDto>> SubmitAttemptAsync(int userId, int lessonId, SubmitQuizAttemptDto dto);
    Task<ApiResponse<List<QuizAttemptDto>>> GetMyAttemptsAsync(int userId, int lessonId);
}
