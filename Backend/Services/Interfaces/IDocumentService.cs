using Backend.Common;
using Backend.DTOs;

namespace Backend.Services.Interfaces;

public interface IDocumentService
{
    Task<ApiResponse<List<DocumentResponseDto>>> GetAllAsync();
    Task<ApiResponse<DocumentResponseDto>> GetByIdAsync(int id);
    Task<ApiResponse<DocumentResponseDto>> CreateAsync(CreateDocumentDto dto);
    Task<ApiResponse<DocumentResponseDto>> UpdateAsync(int id, UpdateDocumentDto dto);
    Task<ApiResponse<object?>> DeleteAsync(int id);

    // Dành cho học viên xem trực tiếp ở trang "Tài liệu chung" (không cần qua khóa học).
    Task<ApiResponse<List<StudentDocumentDto>>> GetAllForStudentAsync();
    Task<ApiResponse<StudentDocumentDto>> GetForStudentAsync(int id);
}
