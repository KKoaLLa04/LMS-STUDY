using Backend.DTOs;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LessonsController : ControllerBase
{
    private readonly ILessonService _lessonService;

    public LessonsController(ILessonService lessonService)
    {
        _lessonService = lessonService;
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
