using Backend.Common;
using Backend.DTOs;

namespace Backend.Services.Interfaces;

public interface IVideoPlaybackService
{
    /// <summary>
    /// Sinh URL phát video của một bài học — chỉ Admin hoặc học viên đã ghi danh khóa học chứa
    /// bài học đó mới được phép.
    /// </summary>
    Task<ApiResponse<PlaybackUrlResultDto>> GetLessonVideoUrlAsync(int lessonId, int userId, bool isAdmin);

    /// <summary>
    /// Sinh URL phát video giới thiệu khóa học — công khai, không cần đăng nhập/ghi danh.
    /// </summary>
    Task<ApiResponse<PlaybackUrlResultDto>> GetCoursePreviewUrlAsync(int courseId);
}
