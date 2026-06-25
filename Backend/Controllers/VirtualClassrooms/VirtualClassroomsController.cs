using Backend.DTOs;
using Backend.Services.VirtualClassrooms.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.VirtualClassrooms;

[ApiController]
[Route("api/[controller]")]
public class VirtualClassroomsController : ControllerBase
{
    private readonly IVirtualClassroomService _service;

    public VirtualClassroomsController(IVirtualClassroomService service)
    {
        _service = service;
    }

    /// <summary>Lấy danh sách phòng học theo khóa học</summary>
    [HttpGet("by-course/{courseId}")]
    public async Task<IActionResult> GetByCourse(
        int courseId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _service.GetClassroomsByCourseAsync(courseId, page, pageSize);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>Lấy chi tiết phòng học theo ID</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetClassroomByIdAsync(id);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>Tạo phòng học trực tuyến mới</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateVirtualClassroomDto dto)
    {
        var result = await _service.CreateClassroomAsync(dto);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>Cập nhật thông tin phòng học</summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateVirtualClassroomDto dto)
    {
        var result = await _service.UpdateClassroomAsync(id, dto);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>Cập nhật trạng thái phòng học (Scheduled, Active, Ended, Cancelled)</summary>
    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromQuery] string status)
    {
        var result = await _service.UpdateStatusAsync(id, status);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>Xóa phòng học</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteClassroomAsync(id);
        return StatusCode(result.HttpStatusCode, result);
    }
}
