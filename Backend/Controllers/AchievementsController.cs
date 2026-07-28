using System.Security.Claims;
using Backend.DTOs;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AchievementsController : ControllerBase
{
    private readonly IAchievementService _achievementService;

    public AchievementsController(IAchievementService achievementService)
    {
        _achievementService = achievementService;
    }

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>
    /// [Admin/User] Lấy danh sách huy hiệu thành tích
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAchievements()
    {
        var result = await _achievementService.GetAllAsync();
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>
    /// [Admin/User] Lấy chi tiết huy hiệu thành tích
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetAchievement(int id)
    {
        var result = await _achievementService.GetByIdAsync(id);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>
    /// [Admin] Tạo mới huy hiệu thành tích
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateAchievement([FromBody] CreateAchievementDto dto)
    {
        var result = await _achievementService.CreateAsync(dto);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>
    /// [Admin] Cập nhật huy hiệu thành tích
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateAchievement(int id, [FromBody] UpdateAchievementDto dto)
    {
        var result = await _achievementService.UpdateAsync(id, dto);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>
    /// [Admin] Xóa huy hiệu thành tích
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteAchievement(int id)
    {
        var result = await _achievementService.DeleteAsync(id);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>
    /// [User] Danh sách huy hiệu kèm trạng thái mở khóa thật của bản thân
    /// </summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetMyAchievements()
    {
        var result = await _achievementService.GetMyAchievementsAsync(CurrentUserId);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>
    /// [Admin] Mở khóa một huy hiệu cho một học sinh cụ thể (cộng điểm thưởng tương ứng)
    /// </summary>
    [HttpPost("{id:int}/unlock/{userId:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UnlockForUser(int id, int userId)
    {
        var result = await _achievementService.UnlockForUserAsync(userId, id);
        return StatusCode(result.HttpStatusCode, result);
    }
}
