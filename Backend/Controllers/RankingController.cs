using System.Security.Claims;
using Backend.DTOs;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RankingController : ControllerBase
{
    private readonly IRankingService _rankingService;

    public RankingController(IRankingService rankingService)
    {
        _rankingService = rankingService;
    }

    // Dùng riêng cho endpoint AllowAnonymous bên dưới — khách chưa đăng nhập không có claim
    // NameIdentifier, CurrentUserId (non-null) sẽ ném lỗi nếu gọi thẳng trong trường hợp đó.
    private int? CurrentUserIdOrNull =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    /// <summary>
    /// [Public] Bảng xếp hạng học sinh — toàn hệ thống (bỏ trống courseId) hoặc theo lớp/khóa học.
    /// Mở cho khách chưa đăng nhập xem, cùng chính sách với trang danh sách khóa học; IsMe/IsFollowing
    /// chỉ có ý nghĩa khi đã đăng nhập.
    /// period: week | month | all (mặc định all)
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetLeaderboard(
        [FromQuery] string period = "all",
        [FromQuery] int? courseId = null,
        [FromQuery] int? khoiHocId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (!Enum.TryParse<RankPeriod>(period, ignoreCase: true, out var parsedPeriod))
            parsedPeriod = RankPeriod.All;

        var result = await _rankingService.GetLeaderboardAsync(parsedPeriod, courseId, khoiHocId, CurrentUserIdOrNull, page, pageSize);
        return StatusCode(result.HttpStatusCode, result);
    }
}
