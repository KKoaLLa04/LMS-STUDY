using Backend.Common;
using Backend.DTOs;

namespace Backend.Services.Interfaces;

public interface ILessonProgressService
{
    Task<ApiResponse<LessonProgressDto>> UpdateProgressAsync(int userId, int lessonId, UpdateLessonProgressDto dto);
    Task<ApiResponse<List<LessonProgressDto>>> GetMyProgressForCourseAsync(int userId, int courseId);
}
