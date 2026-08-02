using System.Security.Claims;
using Backend.DTOs;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CourseReviewsController : ControllerBase
{
    private readonly ICourseReviewService _service;

    public CourseReviewsController(ICourseReviewService service)
    {
        _service = service;
    }

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // Dùng riêng cho 2 endpoint AllowAnonymous bên dưới — khách chưa đăng nhập không có claim
    // NameIdentifier, CurrentUserId (non-null) sẽ ném lỗi nếu gọi thẳng trong trường hợp đó.
    private int? CurrentUserIdOrNull =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    /// <summary>[Public] Tổng hợp điểm đánh giá trung bình + phân bố theo số sao của một khóa học —
    /// mở cho khách chưa đăng nhập xem, cùng chính sách với trang chi tiết khóa học.</summary>
    [HttpGet("summary/{courseId:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSummary(int courseId)
    {
        var result = await _service.GetSummaryAsync(courseId);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>[Public] Danh sách đánh giá của một khóa học (phân trang) — mở cho khách chưa đăng
    /// nhập xem; IsMine chỉ true khi có đăng nhập và đúng là đánh giá của người gọi.</summary>
    [HttpGet("by-course/{courseId:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByCourse(int courseId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _service.GetByCourseAsync(courseId, page, pageSize, CurrentUserIdOrNull);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>[User] Gửi/cập nhật đánh giá của bản thân cho một khóa học — chỉ học sinh đã ghi danh</summary>
    [HttpPost("{courseId:int}")]
    public async Task<IActionResult> CreateOrUpdate(int courseId, [FromBody] CreateCourseReviewDto dto)
    {
        var result = await _service.CreateOrUpdateAsync(courseId, CurrentUserId, dto);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>[User] Xóa đánh giá của bản thân (hoặc Admin xóa bất kỳ đánh giá nào)</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteAsync(id, CurrentUserId, User.IsInRole("Admin"));
        return StatusCode(result.HttpStatusCode, result);
    }
}
