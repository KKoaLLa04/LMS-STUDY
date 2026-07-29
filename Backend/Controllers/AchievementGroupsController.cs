using Backend.DTOs;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/achievement-groups")]
[Authorize]
public class AchievementGroupsController : ControllerBase
{
    private readonly IAchievementGroupService _groupService;

    public AchievementGroupsController(IAchievementGroupService groupService)
    {
        _groupService = groupService;
    }

    /// <summary>
    /// [Admin/User] Lấy danh sách nhóm huy hiệu
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetGroups()
    {
        var result = await _groupService.GetAllAsync();
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>
    /// [Admin] Tạo mới nhóm huy hiệu
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateGroup([FromBody] CreateAchievementGroupDto dto)
    {
        var result = await _groupService.CreateAsync(dto);
        return StatusCode(result.HttpStatusCode, result);
    }
}
