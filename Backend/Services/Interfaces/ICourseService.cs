using Backend.Common;
using Backend.DTOs;

namespace Backend.Services.Interfaces;

public interface ICourseService
{
    Task<ApiResponse<PagedResultDto<CourseListItemDto>>> GetCoursesAsync(int page, int pageSize, string? keyword);
    Task<ApiResponse<CourseDetailDto>> GetCourseByIdAsync(int id);
    Task<ApiResponse<CourseListItemDto>> CreateCourseAsync(CreateCourseDto dto);
    Task<ApiResponse<CourseListItemDto>> UpdateCourseAsync(int id, UpdateCourseDto dto);
    Task<ApiResponse<object?>> DeleteCourseAsync(int id);
}
