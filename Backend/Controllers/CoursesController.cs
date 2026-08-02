using Backend.Authorization;
using Backend.DTOs;
using Backend.Models;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CoursesController : ControllerBase
{
    private readonly ICourseService _courseService;

    public CoursesController(ICourseService courseService)
    {
        _courseService = courseService;
    }

    /// <summary>
    /// [Public] Lấy danh sách khóa học có phân trang và tìm kiếm.
    /// Khách chưa đăng nhập/User chỉ thấy khóa học đã Published.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetCourses(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? keyword = null)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 10;

        var result = await _courseService.GetCoursesAsync(page, pageSize, keyword, User.IsInRole("Admin"));
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>
    /// [Public] Lấy chi tiết khóa học kèm danh sách Sections và Lessons.
    /// Khách chưa đăng nhập/User chỉ xem được khóa học đã Published.
    /// </summary>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCourse(int id)
    {
        var result = await _courseService.GetCourseByIdAsync(id, User.IsInRole("Admin"));
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>
    /// [Admin] Tạo mới khóa học
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Teacher")]
    [RequireTeacherPermission(PermissionModule.Courses, PermissionAction.Create)]
    public async Task<IActionResult> CreateCourse([FromBody] CreateCourseDto dto)
    {
        var result = await _courseService.CreateCourseAsync(dto);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>
    /// [Admin] Cập nhật khóa học
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Teacher")]
    [RequireTeacherPermission(PermissionModule.Courses, PermissionAction.Update)]
    public async Task<IActionResult> UpdateCourse(int id, [FromBody] UpdateCourseDto dto)
    {
        var result = await _courseService.UpdateCourseAsync(id, dto);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>
    /// [Admin] Xóa khóa học (cascade xóa cả Sections và Lessons)
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,Teacher")]
    [RequireTeacherPermission(PermissionModule.Courses, PermissionAction.Delete)]
    public async Task<IActionResult> DeleteCourse(int id)
    {
        var result = await _courseService.DeleteCourseAsync(id);
        return StatusCode(result.HttpStatusCode, result);
    }
}
