using Backend.Authorization;
using Backend.DTOs;
using Backend.Models;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Teacher")]
public class LessonsController : ControllerBase
{
    private readonly ILessonService _lessonService;

    public LessonsController(ILessonService lessonService)
    {
        _lessonService = lessonService;
    }

    /// <summary>
    /// [Admin/Teacher] Lấy danh sách bài học theo loại (Document/Quiz) trên toàn bộ khóa học —
    /// dùng cho các trang quản trị "Tài liệu"/"Quiz" độc lập
    /// </summary>
    [HttpGet]
    [RequireTeacherPermission(PermissionModule.Courses, PermissionAction.View)]
    public async Task<IActionResult> GetByType([FromQuery] string lessonType)
    {
        var result = await _lessonService.GetByTypeAsync(lessonType);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>
    /// [Admin/Teacher] Lấy chi tiết một bài học
    /// </summary>
    [HttpGet("{id:int}")]
    [RequireTeacherPermission(PermissionModule.Courses, PermissionAction.View)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _lessonService.GetByIdAsync(id);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>
    /// [Admin/Teacher] Tạo mới bài học thuộc một chương học
    /// </summary>
    [HttpPost]
    [RequireTeacherPermission(PermissionModule.Courses, PermissionAction.Create)]
    public async Task<IActionResult> CreateLesson([FromBody] CreateLessonDto dto)
    {
        var result = await _lessonService.CreateLessonAsync(dto);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>
    /// [Admin/Teacher] Cập nhật bài học
    /// </summary>
    [HttpPut("{id:int}")]
    [RequireTeacherPermission(PermissionModule.Courses, PermissionAction.Update)]
    public async Task<IActionResult> UpdateLesson(int id, [FromBody] UpdateLessonDto dto)
    {
        var result = await _lessonService.UpdateLessonAsync(id, dto);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>
    /// [Admin/Teacher] Xóa bài học
    /// </summary>
    [HttpDelete("{id:int}")]
    [RequireTeacherPermission(PermissionModule.Courses, PermissionAction.Delete)]
    public async Task<IActionResult> DeleteLesson(int id)
    {
        var result = await _lessonService.DeleteLessonAsync(id);
        return StatusCode(result.HttpStatusCode, result);
    }
}
