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
public class CourseCategoriesController : ControllerBase
{
    private readonly ICourseCategoryService _categoryService;

    public CourseCategoriesController(ICourseCategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    /// <summary>
    /// [Public] Lấy danh sách danh mục khóa học
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetCategories()
    {
        var result = await _categoryService.GetAllAsync();
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>
    /// [Public] Lấy chi tiết danh mục khóa học
    /// </summary>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCategory(int id)
    {
        var result = await _categoryService.GetByIdAsync(id);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>
    /// [Admin] Tạo mới danh mục khóa học
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Teacher")]
    [RequireTeacherPermission(PermissionModule.CourseCategories, PermissionAction.Create)]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCourseCategoryDto dto)
    {
        var result = await _categoryService.CreateAsync(dto);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>
    /// [Admin] Cập nhật danh mục khóa học
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Teacher")]
    [RequireTeacherPermission(PermissionModule.CourseCategories, PermissionAction.Update)]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCourseCategoryDto dto)
    {
        var result = await _categoryService.UpdateAsync(id, dto);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>
    /// [Admin] Xóa danh mục khóa học
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,Teacher")]
    [RequireTeacherPermission(PermissionModule.CourseCategories, PermissionAction.Delete)]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var result = await _categoryService.DeleteAsync(id);
        return StatusCode(result.HttpStatusCode, result);
    }
}
