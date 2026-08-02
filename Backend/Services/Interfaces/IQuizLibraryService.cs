using Backend.Common;
using Backend.DTOs;

namespace Backend.Services.Interfaces;

// CRUD phần "vỏ" của Quiz dùng chung (Title/Description). Câu hỏi bên trong quiz vẫn do
// IQuizQuestionService quản lý (GetForAdminAsync/GetForStudentAsync/ReplaceQuestionsAsync theo quizId).
public interface IQuizLibraryService
{
    Task<ApiResponse<List<QuizResponseDto>>> GetAllAsync();
    Task<ApiResponse<QuizResponseDto>> GetByIdAsync(int id);
    Task<ApiResponse<QuizResponseDto>> CreateAsync(CreateQuizDto dto);
    Task<ApiResponse<QuizResponseDto>> UpdateAsync(int id, UpdateQuizDto dto);
    Task<ApiResponse<object?>> DeleteAsync(int id);

    // Dành cho học viên xem trực tiếp ở trang "Quiz chung" (không cần qua khóa học). userId dùng
    // để tính HasAttempted/BestScorePercent riêng cho từng người xem; null => khách chưa đăng
    // nhập xem danh sách công khai, HasAttempted/BestScorePercent luôn về mặc định (false/null).
    Task<ApiResponse<List<StudentQuizDto>>> GetAllForStudentAsync(int? userId);
    Task<ApiResponse<StudentQuizDto>> GetForStudentAsync(int id, int? userId);
}
