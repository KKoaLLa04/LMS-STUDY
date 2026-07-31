using Backend.Common;
using Backend.DTOs;

namespace Backend.Services.Interfaces;

public interface ICourseReviewService
{
    Task<ApiResponse<CourseRatingSummaryDto>> GetSummaryAsync(int courseId);
    Task<ApiResponse<PagedResultDto<CourseReviewDto>>> GetByCourseAsync(int courseId, int page, int pageSize, int? currentUserId);
    Task<ApiResponse<CourseReviewDto>> CreateOrUpdateAsync(int courseId, int userId, CreateCourseReviewDto dto);
    Task<ApiResponse<object?>> DeleteAsync(int id, int currentUserId, bool isAdmin);
}
