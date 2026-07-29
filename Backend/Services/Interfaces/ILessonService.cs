using Backend.Common;
using Backend.DTOs;

namespace Backend.Services.Interfaces;

public interface ILessonService
{
    Task<ApiResponse<LessonResponseDto>> CreateLessonAsync(CreateLessonDto dto);
    Task<ApiResponse<LessonResponseDto>> UpdateLessonAsync(int id, UpdateLessonDto dto);
    Task<ApiResponse<object?>> DeleteLessonAsync(int id);

    Task<ApiResponse<LessonResponseDto>> GetByIdAsync(int id);

    // lessonType: "Document" hoặc "Quiz" — dùng cho các trang quản trị độc lập.
    Task<ApiResponse<List<LessonAdminListItemDto>>> GetByTypeAsync(string lessonType);
}
