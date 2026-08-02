using Backend.DTOs;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

// Quản lý nhóm quyền + gán thành viên luôn là đặc quyền riêng của Admin — không nằm trong hệ
// thống phân quyền module (RequireTeacherPermission), cùng lý do như quản lý tài khoản Teacher:
// đây là nơi cấp quyền, không thể tự cấp cho chính mình.
[ApiController]
[Route("api/permission-groups")]
[Authorize(Roles = "Admin")]
public class PermissionGroupsController : ControllerBase
{
    private readonly IPermissionGroupService _service;

    public PermissionGroupsController(IPermissionGroupService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();
        return StatusCode(result.HttpStatusCode, result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return StatusCode(result.HttpStatusCode, result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePermissionGroupDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return StatusCode(result.HttpStatusCode, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePermissionGroupDto dto)
    {
        var result = await _service.UpdateAsync(id, dto);
        return StatusCode(result.HttpStatusCode, result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteAsync(id);
        return StatusCode(result.HttpStatusCode, result);
    }

    [HttpPut("{id:int}/members")]
    public async Task<IActionResult> SetMembers(int id, [FromBody] SetPermissionGroupMembersDto dto)
    {
        var result = await _service.SetMembersAsync(id, dto.UserIds);
        return StatusCode(result.HttpStatusCode, result);
    }
}
