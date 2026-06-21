using Backend.DTOs;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SectionsController : ControllerBase
{
    private readonly ISectionService _sectionService;

    public SectionsController(ISectionService sectionService)
    {
        _sectionService = sectionService;
    }

    /// <summary>
    /// [Admin] Tạo mới chương học thuộc một khóa học
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateSection([FromBody] CreateSectionDto dto)
    {
        var result = await _sectionService.CreateSectionAsync(dto);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>
    /// [Admin] Cập nhật chương học
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateSection(int id, [FromBody] UpdateSectionDto dto)
    {
        var result = await _sectionService.UpdateSectionAsync(id, dto);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>
    /// [Admin] Xóa chương học (cascade xóa cả Lessons bên trong)
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteSection(int id)
    {
        var result = await _sectionService.DeleteSectionAsync(id);
        return StatusCode(result.HttpStatusCode, result);
    }
}
