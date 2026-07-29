using System.Security.Claims;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/follows")]
[Authorize]
public class FollowsController : ControllerBase
{
    private readonly IFollowService _followService;

    public FollowsController(IFollowService followService)
    {
        _followService = followService;
    }

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>[User] Theo dõi một người dùng khác</summary>
    [HttpPost("{userId:int}")]
    public async Task<IActionResult> Follow(int userId)
    {
        var result = await _followService.FollowAsync(CurrentUserId, userId);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>[User] Bỏ theo dõi một người dùng</summary>
    [HttpDelete("{userId:int}")]
    public async Task<IActionResult> Unfollow(int userId)
    {
        var result = await _followService.UnfollowAsync(CurrentUserId, userId);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>[User] Trạng thái theo dõi + số người theo dõi/đang theo dõi của một người dùng</summary>
    [HttpGet("{userId:int}/status")]
    public async Task<IActionResult> GetStatus(int userId)
    {
        var result = await _followService.GetStatusAsync(CurrentUserId, userId);
        return StatusCode(result.HttpStatusCode, result);
    }
}
