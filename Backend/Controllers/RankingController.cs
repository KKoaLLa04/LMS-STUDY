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

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>
    /// [User] Bảng xếp hạng học sinh — toàn hệ thống (bỏ trống courseId) hoặc theo lớp/khóa học.
    /// period: week | month | all (mặc định all)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetLeaderboard(
        [FromQuery] string period = "all",
        [FromQuery] int? courseId = null,
        [FromQuery] int? khoiHocId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (!Enum.TryParse<RankPeriod>(period, ignoreCase: true, out var parsedPeriod))
            parsedPeriod = RankPeriod.All;

        var result = await _rankingService.GetLeaderboardAsync(parsedPeriod, courseId, khoiHocId, CurrentUserId, page, pageSize);
        return StatusCode(result.HttpStatusCode, result);
    }
}
