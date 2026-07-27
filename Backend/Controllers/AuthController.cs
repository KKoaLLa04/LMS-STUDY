using System.Security.Claims;
using Backend.Common;
using Backend.DTOs;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Đăng nhập, trả về JWT token nếu thông tin hợp lệ
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var result = await _authService.LoginAsync(dto);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>
    /// Lấy thông tin người dùng đang đăng nhập (dựa trên JWT token)
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _authService.GetCurrentUserAsync(userId);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>
    /// Đăng xuất — JWT không lưu session phía server nên chỉ xác nhận,
    /// client tự xóa token đã lưu (localStorage) sau khi gọi API này.
    /// </summary>
    [Authorize]
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        return Ok(ApiResponse<object?>.Ok(null, "Đăng xuất thành công"));
    }
}
