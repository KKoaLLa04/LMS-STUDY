using System.Security.Claims;
using Backend.DTOs;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

public class EnrollDto
{
    public int CourseId { get; set; }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EnrollmentsController : ControllerBase
{
    private readonly IEnrollmentService _enrollmentService;

    public EnrollmentsController(IEnrollmentService enrollmentService)
    {
        _enrollmentService = enrollmentService;
    }

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>[User] Ghi danh bản thân vào một khóa học</summary>
    [HttpPost]
    public async Task<IActionResult> Enroll([FromBody] EnrollDto dto)
    {
        var result = await _enrollmentService.EnrollAsync(CurrentUserId, dto.CourseId);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>[User] Danh sách khóa học bản thân đã ghi danh</summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetMine()
    {
        var result = await _enrollmentService.GetMyEnrollmentsAsync(CurrentUserId);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>[Admin/Teacher] Danh sách học sinh đã ghi danh một khóa học</summary>
    [HttpGet("course/{courseId:int}")]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> GetByCourse(int courseId)
    {
        var result = await _enrollmentService.GetByCourseAsync(courseId);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>[Admin/Teacher] Chủ động ghi danh một học sinh bất kỳ vào một khóa học bất kỳ —
    /// dùng khi cấp quyền truy cập ngoài luồng tự đăng ký của học sinh.</summary>
    [HttpPost("admin")]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> AdminEnroll([FromBody] AdminEnrollDto dto)
    {
        var result = await _enrollmentService.EnrollAsync(dto.UserId, dto.CourseId);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>[Admin/Teacher] Hủy ghi danh một học sinh bất kỳ khỏi một khóa học (không giới hạn bản thân).</summary>
    [HttpDelete("admin/{courseId:int}/{userId:int}")]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> AdminUnenroll(int courseId, int userId)
    {
        var result = await _enrollmentService.UnenrollAsync(userId, courseId);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>[User] Hủy ghi danh khỏi một khóa học</summary>
    [HttpDelete("{courseId:int}")]
    public async Task<IActionResult> Unenroll(int courseId)
    {
        var result = await _enrollmentService.UnenrollAsync(CurrentUserId, courseId);
        return StatusCode(result.HttpStatusCode, result);
    }
}
