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
public class SectionsController : ControllerBase
{
    private readonly ISectionService _sectionService;

    public SectionsController(ISectionService sectionService)
    {
        _sectionService = sectionService;
    }

    /// <summary>
    /// [Admin/Teacher] Tạo mới chương học thuộc một khóa học
    /// </summary>
    [HttpPost]
    [RequireTeacherPermission(PermissionModule.Courses, PermissionAction.Create)]
    public async Task<IActionResult> CreateSection([FromBody] CreateSectionDto dto)
    {
        var result = await _sectionService.CreateSectionAsync(dto);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>
    /// [Admin/Teacher] Cập nhật chương học
    /// </summary>
    [HttpPut("{id:int}")]
    [RequireTeacherPermission(PermissionModule.Courses, PermissionAction.Update)]
    public async Task<IActionResult> UpdateSection(int id, [FromBody] UpdateSectionDto dto)
    {
        var result = await _sectionService.UpdateSectionAsync(id, dto);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>
    /// [Admin/Teacher] Xóa chương học (cascade xóa cả Lessons bên trong)
    /// </summary>
    [HttpDelete("{id:int}")]
    [RequireTeacherPermission(PermissionModule.Courses, PermissionAction.Delete)]
    public async Task<IActionResult> DeleteSection(int id)
    {
        var result = await _sectionService.DeleteSectionAsync(id);
        return StatusCode(result.HttpStatusCode, result);
    }
}
