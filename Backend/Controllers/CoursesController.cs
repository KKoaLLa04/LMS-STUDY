using Backend.DTOs;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CoursesController : ControllerBase
{
    private readonly ICourseService _courseService;

    public CoursesController(ICourseService courseService)
    {
        _courseService = courseService;
    }

    /// <summary>
    /// [Admin/User] Lấy danh sách khóa học có phân trang và tìm kiếm
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetCourses(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? keyword = null)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 10;

        var result = await _courseService.GetCoursesAsync(page, pageSize, keyword);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>
    /// [User] Lấy chi tiết khóa học kèm danh sách Sections và Lessons
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetCourse(int id)
    {
        var result = await _courseService.GetCourseByIdAsync(id);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>
    /// [Admin] Tạo mới khóa học
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateCourse([FromBody] CreateCourseDto dto)
    {
        var result = await _courseService.CreateCourseAsync(dto);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>
    /// [Admin] Cập nhật khóa học
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateCourse(int id, [FromBody] UpdateCourseDto dto)
    {
        var result = await _courseService.UpdateCourseAsync(id, dto);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>
    /// [Admin] Xóa khóa học (cascade xóa cả Sections và Lessons)
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteCourse(int id)
    {
        var result = await _courseService.DeleteCourseAsync(id);
        return StatusCode(result.HttpStatusCode, result);
    }
}
