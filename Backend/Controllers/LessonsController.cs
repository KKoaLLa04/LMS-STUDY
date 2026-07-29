using Backend.DTOs;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class LessonsController : ControllerBase
{
    private readonly ILessonService _lessonService;

    public LessonsController(ILessonService lessonService)
    {
        _lessonService = lessonService;
    }

    /// <summary>
    /// [Admin] Lấy danh sách bài học theo loại (Document/Quiz) trên toàn bộ khóa học —
    /// dùng cho các trang quản trị "Tài liệu"/"Quiz" độc lập
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetByType([FromQuery] string lessonType)
    {
        var result = await _lessonService.GetByTypeAsync(lessonType);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>
    /// [Admin] Lấy chi tiết một bài học
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _lessonService.GetByIdAsync(id);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>
    /// [Admin] Tạo mới bài học thuộc một chương học
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateLesson([FromBody] CreateLessonDto dto)
    {
        var result = await _lessonService.CreateLessonAsync(dto);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>
    /// [Admin] Cập nhật bài học
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateLesson(int id, [FromBody] UpdateLessonDto dto)
    {
        var result = await _lessonService.UpdateLessonAsync(id, dto);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>
    /// [Admin] Xóa bài học
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteLesson(int id)
    {
        var result = await _lessonService.DeleteLessonAsync(id);
        return StatusCode(result.HttpStatusCode, result);
    }
}
