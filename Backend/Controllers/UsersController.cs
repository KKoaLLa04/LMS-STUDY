using Backend.DTOs;
using Backend.Models;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// [Admin] Lấy danh sách người dùng, có thể lọc theo vai trò (?role=Teacher|Student|Admin)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetUsers([FromQuery] UserRole? role)
    {
        var result = await _userService.GetByRoleAsync(role);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>
    /// [Admin] Lấy chi tiết người dùng
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var result = await _userService.GetByIdAsync(id);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>
    /// [Admin] Tạo tài khoản mới (giảng viên / học sinh / quản trị viên)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
    {
        var result = await _userService.CreateAsync(dto);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>
    /// [Admin] Cập nhật tài khoản
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto dto)
    {
        var result = await _userService.UpdateAsync(id, dto);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>
    /// [Admin] Xóa tài khoản
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var result = await _userService.DeleteAsync(id);
        return StatusCode(result.HttpStatusCode, result);
    }
}
