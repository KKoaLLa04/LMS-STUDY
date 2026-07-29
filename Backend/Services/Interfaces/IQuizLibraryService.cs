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

    // Dành cho học viên xem trực tiếp ở trang "Quiz chung" (không cần qua khóa học).
    Task<ApiResponse<List<StudentQuizDto>>> GetAllForStudentAsync();
    Task<ApiResponse<StudentQuizDto>> GetForStudentAsync(int id);
}
