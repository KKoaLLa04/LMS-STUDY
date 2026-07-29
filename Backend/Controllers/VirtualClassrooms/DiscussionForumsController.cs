using System.Security.Claims;
using Backend.DTOs;
using Backend.Services.VirtualClassrooms.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.VirtualClassrooms;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class DiscussionForumsController : ControllerBase
{
    private readonly IDiscussionForumService _service;

    public DiscussionForumsController(IDiscussionForumService service)
    {
        _service = service;
    }

    // Người gọi hiện chỉ có thể là Admin (class-level [Authorize(Roles = "Admin")]) — vẫn lấy Id
    // để gắn UserId lên bài viết, sẵn sàng cho khi luồng học sinh tự đăng bài được mở ra sau này.
    private int? CurrentUserId =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    /// <summary>[User] Lấy danh sách bài viết thảo luận theo khóa học — mở cho mọi user đã đăng nhập
    /// (chỉ xem, đăng bài vẫn giới hạn Admin), ghi đè [Authorize(Roles = "Admin")] ở cấp class.</summary>
    [HttpGet("by-course/{courseId}")]
    [Authorize]
    public async Task<IActionResult> GetByCourse(
        int courseId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? keyword = null)
    {
        var result = await _service.GetPostsByCourseAsync(courseId, page, pageSize, keyword, CurrentUserId);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>[User] Lấy chi tiết bài viết kèm các replies — mở cho mọi user đã đăng nhập (xem thư trả lời).</summary>
    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetPostByIdAsync(id, CurrentUserId);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>Tạo bài viết thảo luận mới</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDiscussionPostDto dto)
    {
        var result = await _service.CreatePostAsync(dto, CurrentUserId);
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
        var result = await _service.ReplyToPostAsync(parentPostId, dto, CurrentUserId);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>Xóa bài viết thảo luận</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeletePostAsync(id);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>[User] Thích bài viết thảo luận — mở cho mọi user đã đăng nhập, ghi đè
    /// [Authorize(Roles = "Admin")] ở cấp class (đăng bài vẫn đang giới hạn Admin, nhưng thích
    /// bài viết thì không cần).</summary>
    [HttpPost("{id}/like")]
    [Authorize]
    public async Task<IActionResult> Like(int id)
    {
        if (CurrentUserId is not int userId)
            return Unauthorized();

        var result = await _service.LikePostAsync(id, userId);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>[User] Bỏ thích bài viết thảo luận</summary>
    [HttpDelete("{id}/like")]
    [Authorize]
    public async Task<IActionResult> Unlike(int id)
    {
        if (CurrentUserId is not int userId)
            return Unauthorized();

        var result = await _service.UnlikePostAsync(id, userId);
        return StatusCode(result.HttpStatusCode, result);
    }
}
