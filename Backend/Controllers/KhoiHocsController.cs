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
public class KhoiHocsController : ControllerBase
{
    private readonly IKhoiHocService _khoiHocService;

    public KhoiHocsController(IKhoiHocService khoiHocService)
    {
        _khoiHocService = khoiHocService;
    }

    /// <summary>
    /// [Public] Lấy danh sách khối học — mở cho khách chưa đăng nhập xem (dùng làm bộ lọc khối
    /// trên trang bảng xếp hạng công khai).
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetKhoiHocs()
    {
        var result = await _khoiHocService.GetAllAsync();
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>
    /// [Public] Lấy chi tiết khối học
    /// </summary>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetKhoiHoc(int id)
    {
        var result = await _khoiHocService.GetByIdAsync(id);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>
    /// [Admin] Tạo mới khối học
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Teacher")]
    [RequireTeacherPermission(PermissionModule.KhoiHocs, PermissionAction.Create)]
    public async Task<IActionResult> CreateKhoiHoc([FromBody] CreateKhoiHocDto dto)
    {
        var result = await _khoiHocService.CreateAsync(dto);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>
    /// [Admin] Cập nhật khối học
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Teacher")]
    [RequireTeacherPermission(PermissionModule.KhoiHocs, PermissionAction.Update)]
    public async Task<IActionResult> UpdateKhoiHoc(int id, [FromBody] UpdateKhoiHocDto dto)
    {
        var result = await _khoiHocService.UpdateAsync(id, dto);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>
    /// [Admin] Xóa khối học
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,Teacher")]
    [RequireTeacherPermission(PermissionModule.KhoiHocs, PermissionAction.Delete)]
    public async Task<IActionResult> DeleteKhoiHoc(int id)
    {
        var result = await _khoiHocService.DeleteAsync(id);
        return StatusCode(result.HttpStatusCode, result);
    }
}
