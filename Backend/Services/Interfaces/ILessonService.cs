using Backend.Common;
using Backend.DTOs;

namespace Backend.Services.Interfaces;

public interface ILessonService
{
    Task<ApiResponse<LessonResponseDto>> CreateLessonAsync(CreateLessonDto dto);
    Task<ApiResponse<LessonResponseDto>> UpdateLessonAsync(int id, UpdateLessonDto dto);
    Task<ApiResponse<object?>> DeleteLessonAsync(int id);
}
