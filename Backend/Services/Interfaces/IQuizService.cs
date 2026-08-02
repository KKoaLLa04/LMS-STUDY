using Backend.Common;
using Backend.DTOs;

namespace Backend.Services.Interfaces;

public interface IQuizService
{
    Task<ApiResponse<QuizAttemptResultDto>> SubmitAttemptAsync(int userId, int quizId, SubmitQuizAttemptDto dto, int? courseId = null);
    Task<ApiResponse<List<QuizAttemptDto>>> GetMyAttemptsAsync(int userId, int quizId);
    /// <summary>Chi tiết đáp án đã chọn + đúng/sai từng câu của một lần làm cụ thể — dùng để khôi
    /// phục lại đúng màn hình kết quả đã nộp khi học sinh tải lại trang.</summary>
    Task<ApiResponse<QuizAttemptResultDto>> GetAttemptDetailAsync(int userId, int attemptId);
}
