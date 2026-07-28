using System.Security.Claims;
using Backend.DTOs;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LessonProgressController : ControllerBase
{
    private readonly ILessonProgressService _lessonProgressService;

    public LessonProgressController(ILessonProgressService lessonProgressService)
    {
        _lessonProgressService = lessonProgressService;
    }

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>[User] Cập nhật tiến độ xem một bài học (gọi định kỳ từ video player)</summary>
    [HttpPut("{lessonId:int}")]
    public async Task<IActionResult> UpdateProgress(int lessonId, [FromBody] UpdateLessonProgressDto dto)
    {
        var result = await _lessonProgressService.UpdateProgressAsync(CurrentUserId, lessonId, dto);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>[User] Tiến độ học tập của bản thân trong một khóa học</summary>
    [HttpGet("course/{courseId:int}")]
    public async Task<IActionResult> GetMyProgress(int courseId)
    {
        var result = await _lessonProgressService.GetMyProgressForCourseAsync(CurrentUserId, courseId);
        return StatusCode(result.HttpStatusCode, result);
    }
}
