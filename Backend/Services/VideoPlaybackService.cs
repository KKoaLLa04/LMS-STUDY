using Backend.Common;
using Backend.Data;
using Backend.DTOs;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class VideoPlaybackService : IVideoPlaybackService
{
    private readonly AppDbContext _context;
    private readonly IR2StorageService _r2StorageService;
    private readonly ILogger<VideoPlaybackService> _logger;

    public VideoPlaybackService(AppDbContext context, IR2StorageService r2StorageService, ILogger<VideoPlaybackService> logger)
    {
        _context = context;
        _r2StorageService = r2StorageService;
        _logger = logger;
    }

    public async Task<ApiResponse<PlaybackUrlResultDto>> GetLessonVideoUrlAsync(int lessonId, int userId, bool isAdmin)
    {
        try
        {
            var lesson = await _context.Lessons
                .Include(l => l.Section)
                .FirstOrDefaultAsync(l => l.Id == lessonId);

            if (lesson == null || string.IsNullOrWhiteSpace(lesson.VideoUrl))
                return ApiResponse<PlaybackUrlResultDto>.NotFound("Không tìm thấy video của bài học");

            if (!isAdmin)
            {
                var isEnrolled = await _context.Enrollments
                    .AnyAsync(e => e.UserId == userId && e.CourseId == lesson.Section.CourseId);
                if (!isEnrolled)
                    return ApiResponse<PlaybackUrlResultDto>.Forbidden("Bạn cần ghi danh khóa học này để xem video");
            }

            var url = await ResolvePlaybackUrlAsync(lesson.VideoUrl);
            return ApiResponse<PlaybackUrlResultDto>.Ok(new PlaybackUrlResultDto { Url = url });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy URL phát video bài học {LessonId}", lessonId);
            return ApiResponse<PlaybackUrlResultDto>.Error("Đã xảy ra lỗi khi lấy URL phát video");
        }
    }

    public async Task<ApiResponse<PlaybackUrlResultDto>> GetCoursePreviewUrlAsync(int courseId)
    {
        try
        {
            var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == courseId);
            if (course == null || string.IsNullOrWhiteSpace(course.PreviewVideoUrl))
                return ApiResponse<PlaybackUrlResultDto>.NotFound("Không tìm thấy video giới thiệu");

            var url = await ResolvePlaybackUrlAsync(course.PreviewVideoUrl);
            return ApiResponse<PlaybackUrlResultDto>.Ok(new PlaybackUrlResultDto { Url = url });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy URL video giới thiệu khóa học {CourseId}", courseId);
            return ApiResponse<PlaybackUrlResultDto>.Error("Đã xảy ra lỗi khi lấy URL video giới thiệu");
        }
    }

    // Video được tạo trước khi có R2 (hoặc admin dán tay link ngoài như Youtube) lưu URL đầy đủ
    // trong DB thay vì R2 key — trả về nguyên văn thay vì cố ký chúng như key của R2.
    private async Task<string> ResolvePlaybackUrlAsync(string videoUrlOrKey)
    {
        if (videoUrlOrKey.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            videoUrlOrKey.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            return videoUrlOrKey;

        return await _r2StorageService.GeneratePresignedPlaybackUrlAsync(videoUrlOrKey);
    }
}
