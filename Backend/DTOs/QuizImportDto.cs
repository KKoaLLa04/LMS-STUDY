namespace Backend.DTOs;

// Kết quả parse file Excel import câu hỏi quiz — không ghi DB, chỉ trả về để admin merge vào
// form (kết hợp với câu hỏi đã nhập tay) trước khi bấm Lưu (PUT /api/quizzes/{id}/questions).
public class QuizImportResultDto
{
    public List<QuizQuestionAdminDto> Questions { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}
