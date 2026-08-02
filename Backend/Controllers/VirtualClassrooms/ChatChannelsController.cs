using Backend.Authorization;
using Backend.DTOs;
using Backend.Models;
using Backend.Services.VirtualClassrooms.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.VirtualClassrooms;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Teacher")]
public class ChatChannelsController : ControllerBase
{
    private readonly IChatChannelService _service;

    public ChatChannelsController(IChatChannelService service)
    {
        _service = service;
    }

    /// <summary>Lấy danh sách kênh chat theo khóa học</summary>
    [HttpGet("by-course/{courseId}")]
    [RequireTeacherPermission(PermissionModule.VirtualClassrooms, PermissionAction.View)]
    public async Task<IActionResult> GetByCourse(int courseId)
    {
        var result = await _service.GetChannelsByCourseAsync(courseId);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>Lấy chi tiết kênh chat</summary>
    [HttpGet("{id}")]
    [RequireTeacherPermission(PermissionModule.VirtualClassrooms, PermissionAction.View)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetChannelByIdAsync(id);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>Tạo kênh chat mới</summary>
    [HttpPost]
    [RequireTeacherPermission(PermissionModule.VirtualClassrooms, PermissionAction.Create)]
    public async Task<IActionResult> Create([FromBody] CreateChatChannelDto dto)
    {
        var result = await _service.CreateChannelAsync(dto);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>Cập nhật thông tin kênh chat</summary>
    [HttpPut("{id}")]
    [RequireTeacherPermission(PermissionModule.VirtualClassrooms, PermissionAction.Update)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateChatChannelDto dto)
    {
        var result = await _service.UpdateChannelAsync(id, dto);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>Xóa kênh chat</summary>
    [HttpDelete("{id}")]
    [RequireTeacherPermission(PermissionModule.VirtualClassrooms, PermissionAction.Delete)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteChannelAsync(id);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>Lấy tin nhắn trong kênh (phân trang, mới nhất trước)</summary>
    [HttpGet("{id}/messages")]
    [RequireTeacherPermission(PermissionModule.VirtualClassrooms, PermissionAction.View)]
    public async Task<IActionResult> GetMessages(
        int id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var result = await _service.GetMessagesAsync(id, page, pageSize);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>Gửi tin nhắn vào kênh</summary>
    [HttpPost("{id}/messages")]
    [RequireTeacherPermission(PermissionModule.VirtualClassrooms, PermissionAction.Create)]
    public async Task<IActionResult> SendMessage(int id, [FromBody] SendMessageDto dto)
    {
        var result = await _service.SendMessageAsync(id, dto);
        return StatusCode(result.HttpStatusCode, result);
    }
}
