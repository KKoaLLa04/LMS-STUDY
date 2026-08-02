using Backend.Authorization;
using Backend.Common;
using Backend.DTOs;
using Backend.Models;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

// Quản lý tài khoản Teacher luôn là đặc quyền riêng của Admin (không nằm trong hệ thống phân
// quyền module) — ngăn một giáo viên tự nâng quyền cho chính mình/đồng nghiệp. Module "Students"
// (phân quyền được) chỉ cho giáo viên thao tác trên các user Role = Student; mọi action dưới đây
// tự chặn (403/400) nếu người gọi là Teacher nhưng đối tượng/role mục tiêu không phải Student.
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>[Public] Danh sách công khai giáo viên (tên/avatar) — dùng cho trang giáo viên phía học sinh,
    /// mở cho cả khách chưa đăng nhập.</summary>
    [HttpGet("teachers/public")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublicTeachers()
    {
        var result = await _userService.GetPublicTeachersAsync();
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>[Public] Thông tin công khai tối thiểu của một người dùng (tên/avatar) — dùng cho trang hồ sơ
    /// giáo viên/học sinh phía học sinh, mở cho cả khách chưa đăng nhập.</summary>
    [HttpGet("public/{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublicUser(int id)
    {
        var result = await _userService.GetPublicByIdAsync(id);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>
    /// [Admin] Lấy danh sách người dùng, có thể lọc theo vai trò (?role=Teacher|Student|Admin).
    /// [Teacher] Chỉ được xem danh sách Student — role query bị ép về Student bất kể giá trị gửi lên.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Teacher")]
    [RequireTeacherPermission(PermissionModule.Students, PermissionAction.View)]
    public async Task<IActionResult> GetUsers([FromQuery] UserRole? role)
    {
        var effectiveRole = User.IsInRole("Admin") ? role : UserRole.Student;
        var result = await _userService.GetByRoleAsync(effectiveRole);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>
    /// [Admin] Lấy danh sách người dùng có phân trang + tìm kiếm (?role=&amp;page=&amp;pageSize=&amp;keyword=).
    /// [Teacher] Chỉ được xem danh sách Student — role query bị ép về Student bất kể giá trị gửi lên.
    /// </summary>
    [HttpGet("paged")]
    [Authorize(Roles = "Admin,Teacher")]
    [RequireTeacherPermission(PermissionModule.Students, PermissionAction.View)]
    public async Task<IActionResult> GetUsersPaged(
        [FromQuery] UserRole? role,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? keyword = null)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 10;

        var effectiveRole = User.IsInRole("Admin") ? role : UserRole.Student;
        var result = await _userService.GetPagedAsync(effectiveRole, page, pageSize, keyword);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>
    /// [Admin] Lấy chi tiết người dùng. [Teacher] Chỉ được xem user Role = Student.
    /// </summary>
    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,Teacher")]
    [RequireTeacherPermission(PermissionModule.Students, PermissionAction.View)]
    public async Task<IActionResult> GetUser(int id)
    {
        var result = await _userService.GetByIdAsync(id);
        if (!User.IsInRole("Admin") && result.Data != null && result.Data.Role != nameof(UserRole.Student))
            return StatusCode(403, ApiResponse<object?>.Forbidden("Bạn không có quyền xem tài khoản này"));

        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>
    /// [Admin] Tạo tài khoản mới (giảng viên / học sinh / quản trị viên).
    /// [Teacher] Chỉ được tạo tài khoản Role = Student, không được đặt Permissions.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Teacher")]
    [RequireTeacherPermission(PermissionModule.Students, PermissionAction.Create)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
    {
        var isAdmin = User.IsInRole("Admin");
        if (!isAdmin && dto.Role != UserRole.Student)
            return StatusCode(403, ApiResponse<object?>.Forbidden("Bạn chỉ có thể tạo tài khoản học sinh"));

        var result = await _userService.CreateAsync(dto, isAdmin);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>
    /// [Admin] Cập nhật tài khoản. [Teacher] Chỉ được cập nhật user hiện tại lẫn mục tiêu đều
    /// Role = Student, không được đặt Permissions.
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Teacher")]
    [RequireTeacherPermission(PermissionModule.Students, PermissionAction.Update)]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto dto)
    {
        var isAdmin = User.IsInRole("Admin");
        if (!isAdmin)
        {
            if (dto.Role != UserRole.Student)
                return StatusCode(403, ApiResponse<object?>.Forbidden("Bạn chỉ có thể cập nhật tài khoản học sinh"));

            var existing = await _userService.GetByIdAsync(id);
            if (existing.Data != null && existing.Data.Role != nameof(UserRole.Student))
                return StatusCode(403, ApiResponse<object?>.Forbidden("Bạn không có quyền cập nhật tài khoản này"));
        }

        var result = await _userService.UpdateAsync(id, dto, isAdmin);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>
    /// [Admin] Xóa tài khoản. [Teacher] Chỉ được xóa user Role = Student.
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,Teacher")]
    [RequireTeacherPermission(PermissionModule.Students, PermissionAction.Delete)]
    public async Task<IActionResult> DeleteUser(int id)
    {
        if (!User.IsInRole("Admin"))
        {
            var existing = await _userService.GetByIdAsync(id);
            if (existing.Data != null && existing.Data.Role != nameof(UserRole.Student))
                return StatusCode(403, ApiResponse<object?>.Forbidden("Bạn không có quyền xóa tài khoản này"));
        }

        var result = await _userService.DeleteAsync(id);
        return StatusCode(result.HttpStatusCode, result);
    }
}
