using Backend.Common;
using Backend.DTOs;

namespace Backend.Services.Interfaces;

public interface IEnrollmentService
{
    Task<ApiResponse<EnrollmentDto>> EnrollAsync(int userId, int courseId);
    Task<ApiResponse<List<EnrollmentDto>>> GetMyEnrollmentsAsync(int userId);
    Task<ApiResponse<List<EnrollmentDto>>> GetByCourseAsync(int courseId);
    Task<ApiResponse<object?>> UnenrollAsync(int userId, int courseId);
}
