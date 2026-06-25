using Backend.DTOs;
using Backend.Services.VirtualClassrooms.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.VirtualClassrooms;

[ApiController]
[Route("api/[controller]")]
public class DiscussionForumsController : ControllerBase
{
    private readonly IDiscussionForumService _service;

    public DiscussionForumsController(IDiscussionForumService service)
    {
        _service = service;
    }

    /// <summary>Lấy danh sách bài viết thảo luận theo khóa học</summary>
    [HttpGet("by-course/{courseId}")]
    public async Task<IActionResult> GetByCourse(
        int courseId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? keyword = null)
    {
        var result = await _service.GetPostsByCourseAsync(courseId, page, pageSize, keyword);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>Lấy chi tiết bài viết kèm các replies</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetPostByIdAsync(id);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>Tạo bài viết thảo luận mới</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDiscussionPostDto dto)
    {
        var result = await _service.CreatePostAsync(dto);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>Cập nhật bài viết thảo luận</summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDiscussionPostDto dto)
    {
        var result = await _service.UpdatePostAsync(id, dto);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>Trả lời bài viết thảo luận</summary>
    [HttpPost("{parentPostId}/reply")]
    public async Task<IActionResult> Reply(int parentPostId, [FromBody] CreateDiscussionPostDto dto)
    {
        var result = await _service.ReplyToPostAsync(parentPostId, dto);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>Xóa bài viết thảo luận</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeletePostAsync(id);
        return StatusCode(result.HttpStatusCode, result);
    }
}
