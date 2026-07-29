using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/achievement-conditions")]
[Authorize]
public class AchievementConditionsController : ControllerBase
{
    private readonly IAchievementConditionService _achievementConditionService;

    public AchievementConditionsController(IAchievementConditionService achievementConditionService)
    {
        _achievementConditionService = achievementConditionService;
    }

    /// <summary>
    /// [Admin/User] Lấy danh sách loại điều kiện đạt được huy hiệu (dùng cho dropdown Frontend)
    /// </summary>
    [HttpGet("types")]
    public async Task<IActionResult> GetTypes()
    {
        var result = await _achievementConditionService.GetTypesAsync();
        return StatusCode(result.HttpStatusCode, result);
    }
}
