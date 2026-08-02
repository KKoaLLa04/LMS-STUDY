using System.Security.Claims;
using Backend.Authorization;
using Backend.DTOs;
using Backend.Models;
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

    // Dùng riêng cho các endpoint AllowAnonymous — khách chưa đăng nhập không có claim
    // NameIdentifier, CurrentUserId (non-null) sẽ ném lỗi nếu gọi thẳng trong trường hợp đó.
    private int? CurrentUserIdOrNull =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    /// <summary>
    /// [Public] Lấy danh sách huy hiệu thành tích
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAchievements()
    {
        var result = await _achievementService.GetAllAsync();
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>
    /// [Public] Lấy chi tiết huy hiệu thành tích
    /// </summary>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAchievement(int id)
    {
        var result = await _achievementService.GetByIdAsync(id);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>
    /// [Admin] Tạo mới huy hiệu thành tích
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Teacher")]
    [RequireTeacherPermission(PermissionModule.Achievements, PermissionAction.Create)]
    public async Task<IActionResult> CreateAchievement([FromBody] CreateAchievementDto dto)
    {
        var result = await _achievementService.CreateAsync(dto);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>
    /// [Admin] Cập nhật huy hiệu thành tích
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Teacher")]
    [RequireTeacherPermission(PermissionModule.Achievements, PermissionAction.Update)]
    public async Task<IActionResult> UpdateAchievement(int id, [FromBody] UpdateAchievementDto dto)
    {
        var result = await _achievementService.UpdateAsync(id, dto);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>
    /// [Admin] Xóa huy hiệu thành tích
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,Teacher")]
    [RequireTeacherPermission(PermissionModule.Achievements, PermissionAction.Delete)]
    public async Task<IActionResult> DeleteAchievement(int id)
    {
        var result = await _achievementService.DeleteAsync(id);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>
    /// [Public] Danh sách huy hiệu kèm trạng thái mở khóa thật của bản thân — khách chưa đăng
    /// nhập vẫn xem được catalogue, chỉ khác là mọi huy hiệu hiển thị ở trạng thái chưa mở khóa.
    /// </summary>
    [HttpGet("me")]
    [AllowAnonymous]
    public async Task<IActionResult> GetMyAchievements()
    {
        var result = await _achievementService.GetMyAchievementsAsync(CurrentUserIdOrNull);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>
    /// [Admin] Mở khóa một huy hiệu cho một học sinh cụ thể (cộng điểm thưởng tương ứng)
    /// </summary>
    [HttpPost("{id:int}/unlock/{userId:int}")]
    [Authorize(Roles = "Admin,Teacher")]
    [RequireTeacherPermission(PermissionModule.Achievements, PermissionAction.Create)]
    public async Task<IActionResult> UnlockForUser(int id, int userId)
    {
        var result = await _achievementService.UnlockForUserAsync(userId, id);
        return StatusCode(result.HttpStatusCode, result);
    }
}
