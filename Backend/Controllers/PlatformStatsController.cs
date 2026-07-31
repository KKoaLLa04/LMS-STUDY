using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PlatformStatsController : ControllerBase
{
    private readonly IPlatformStatsService _service;

    public PlatformStatsController(IPlatformStatsService service)
    {
        _service = service;
    }

    /// <summary>[User] Số liệu tổng quan nền tảng (số học viên/giáo viên/khóa học/giờ học) — dùng cho khối thống kê ở Trang chủ.</summary>
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var result = await _service.GetStatsAsync();
        return StatusCode(result.HttpStatusCode, result);
    }
}
