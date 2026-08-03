using System.Security.Claims;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/video-playback")]
public class VideoPlaybackController : ControllerBase
{
    private readonly IVideoPlaybackService _videoPlaybackService;

    public VideoPlaybackController(IVideoPlaybackService videoPlaybackService)
    {
        _videoPlaybackService = videoPlaybackService;
    }

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>[User] Lấy URL phát video của một bài học — yêu cầu đã ghi danh khóa học (hoặc Admin)</summary>
    [HttpGet("lesson/{lessonId:int}")]
    [Authorize]
    public async Task<IActionResult> GetLessonVideoUrl(int lessonId)
    {
        var result = await _videoPlaybackService.GetLessonVideoUrlAsync(lessonId, CurrentUserId, User.IsInRole("Admin"));
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>[Public] Lấy URL phát video giới thiệu khóa học — không cần đăng nhập</summary>
    [HttpGet("course-preview/{courseId:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCoursePreviewUrl(int courseId)
    {
        var result = await _videoPlaybackService.GetCoursePreviewUrlAsync(courseId);
        return StatusCode(result.HttpStatusCode, result);
    }
}
