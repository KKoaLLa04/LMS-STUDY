using Backend.Common;
using Backend.DTOs;

namespace Backend.Services.Interfaces;

public interface IQuizImportService
{
    // Chỉ đọc/parse file Excel, KHÔNG ghi DB — kết quả trả về để admin merge vào form (kết hợp
    // với câu hỏi đã nhập tay) trước khi bấm Lưu.
    ApiResponse<QuizImportResultDto> ParseExcel(Stream fileStream);

    // Sinh file Excel mẫu (.xlsx) để admin tải về và điền theo đúng cột.
    byte[] BuildTemplate();
}
