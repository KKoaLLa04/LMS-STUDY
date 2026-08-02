using System.Security.Claims;
using Backend.Authorization;
using Backend.DTOs;
using Backend.Models;
using Backend.Services.VirtualClassrooms.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.VirtualClassrooms;

// LƯU Ý: [Authorize] ở method KHÔNG ghi đè [Authorize(Roles = ...)] ở class — ASP.NET Core cộng
// dồn (AND) mọi [Authorize] áp dụng cho 1 action, chỉ [AllowAnonymous] mới thực sự bỏ qua được.
// Vì vậy class-level ở đây để trần [Authorize] (chỉ yêu cầu đăng nhập, không giới hạn role); việc
// giới hạn role/quyền cho Update/Delete do [RequireTeacherPermission] tự đảm nhiệm độc lập bên dưới.
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DiscussionForumsController : ControllerBase
{
    private readonly IDiscussionForumService _service;

    public DiscussionForumsController(IDiscussionForumService service)
    {
        _service = service;
    }

    // null nếu request ẩn danh (GetByCourse/GetById cho phép [AllowAnonymous]).
    private int? CurrentUserId =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    /// <summary>[Public] Lấy danh sách bài viết thảo luận theo khóa học — thảo luận là khu vực bình
    /// luận công khai nên cho phép xem kể cả chưa đăng nhập (đăng bài/trả lời mới cần đăng nhập).</summary>
    [HttpGet("by-course/{courseId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByCourse(
        int courseId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? keyword = null)
    {
        var result = await _service.GetPostsByCourseAsync(courseId, page, pageSize, keyword, CurrentUserId);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>[Public] Lấy chi tiết bài viết kèm các replies — công khai như GetByCourse ở trên.</summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetPostByIdAsync(id, CurrentUserId);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>[User] Tạo bài viết thảo luận mới — mở cho mọi user đã đăng nhập (dạng bình luận
    /// công khai giữa học sinh/giáo viên/quản trị).</summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateDiscussionPostDto dto)
    {
        var result = await _service.CreatePostAsync(dto, CurrentUserId);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>Cập nhật bài viết thảo luận</summary>
    [HttpPut("{id}")]
    [RequireTeacherPermission(PermissionModule.DiscussionForums, PermissionAction.Update)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDiscussionPostDto dto)
    {
        var result = await _service.UpdatePostAsync(id, dto);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>[User] Trả lời bài viết thảo luận — mở cho mọi user đã đăng nhập, cùng chính sách
    /// với Create ở trên.</summary>
    [HttpPost("{parentPostId}/reply")]
    [Authorize]
    public async Task<IActionResult> Reply(int parentPostId, [FromBody] CreateDiscussionPostDto dto)
    {
        var result = await _service.ReplyToPostAsync(parentPostId, dto, CurrentUserId);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>Xóa bài viết thảo luận</summary>
    [HttpDelete("{id}")]
    [RequireTeacherPermission(PermissionModule.DiscussionForums, PermissionAction.Delete)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeletePostAsync(id);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>[User] Thích bài viết thảo luận — mở cho mọi user đã đăng nhập.</summary>
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
