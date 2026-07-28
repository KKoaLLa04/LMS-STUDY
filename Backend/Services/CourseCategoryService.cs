using Backend.Common;
using Backend.Data;
using Backend.DTOs;
using Backend.Models;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class CourseCategoryService : ICourseCategoryService
{
    private readonly AppDbContext _context;
    private readonly ILogger<CourseCategoryService> _logger;

    public CourseCategoryService(AppDbContext context, ILogger<CourseCategoryService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<List<CourseCategoryDto>>> GetAllAsync()
    {
        try
        {
            var items = await _context.CourseCategories
                .OrderBy(c => c.OrderNumber)
                .Select(c => MapToDto(c))
                .ToListAsync();

            return ApiResponse<List<CourseCategoryDto>>.Ok(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách danh mục khóa học");
            return ApiResponse<List<CourseCategoryDto>>.Error("Đã xảy ra lỗi khi lấy danh sách danh mục khóa học");
        }
    }

    public async Task<ApiResponse<CourseCategoryDto>> GetByIdAsync(int id)
    {
        try
        {
            var category = await _context.CourseCategories.FirstOrDefaultAsync(c => c.Id == id);
            if (category == null)
                return ApiResponse<CourseCategoryDto>.NotFound("Danh mục khóa học không tồn tại");

            return ApiResponse<CourseCategoryDto>.Ok(MapToDto(category));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy chi tiết danh mục khóa học {Id}", id);
            return ApiResponse<CourseCategoryDto>.Error("Đã xảy ra lỗi khi lấy thông tin danh mục khóa học");
        }
    }

    public async Task<ApiResponse<CourseCategoryDto>> CreateAsync(CreateCourseCategoryDto dto)
    {
        try
        {
            var code = dto.Code.Trim();

            var codeExists = await _context.CourseCategories.AnyAsync(c => c.Code == code);
            if (codeExists)
                return ApiResponse<CourseCategoryDto>.BadRequest($"Mã danh mục \"{code}\" đã tồn tại");

            var category = new CourseCategory
            {
                Name = dto.Name.Trim(),
                Code = code,
                OrderNumber = dto.OrderNumber
            };

            _context.CourseCategories.Add(category);
            await _context.SaveChangesAsync();

            return ApiResponse<CourseCategoryDto>.Ok(MapToDto(category), "Tạo danh mục khóa học thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tạo danh mục khóa học");
            return ApiResponse<CourseCategoryDto>.Error("Đã xảy ra lỗi khi tạo danh mục khóa học");
        }
    }

    public async Task<ApiResponse<CourseCategoryDto>> UpdateAsync(int id, UpdateCourseCategoryDto dto)
    {
        try
        {
            var category = await _context.CourseCategories.FirstOrDefaultAsync(c => c.Id == id);
            if (category == null)
                return ApiResponse<CourseCategoryDto>.NotFound("Danh mục khóa học không tồn tại");

            var code = dto.Code.Trim();

            var codeExists = await _context.CourseCategories.AnyAsync(c => c.Code == code && c.Id != id);
            if (codeExists)
                return ApiResponse<CourseCategoryDto>.BadRequest($"Mã danh mục \"{code}\" đã tồn tại");

            category.Name = dto.Name.Trim();
            category.Code = code;
            category.OrderNumber = dto.OrderNumber;

            await _context.SaveChangesAsync();

            return ApiResponse<CourseCategoryDto>.Ok(MapToDto(category), "Cập nhật danh mục khóa học thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi cập nhật danh mục khóa học {Id}", id);
            return ApiResponse<CourseCategoryDto>.Error("Đã xảy ra lỗi khi cập nhật danh mục khóa học");
        }
    }

    public async Task<ApiResponse<object?>> DeleteAsync(int id)
    {
        try
        {
            var category = await _context.CourseCategories.FirstOrDefaultAsync(c => c.Id == id);
            if (category == null)
                return ApiResponse<object?>.NotFound("Danh mục khóa học không tồn tại");

            category.IsDeleted = true;
            category.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return ApiResponse<object?>.Ok(null, "Xóa danh mục khóa học thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xóa danh mục khóa học {Id}", id);
            return ApiResponse<object?>.Error("Đã xảy ra lỗi khi xóa danh mục khóa học");
        }
    }

    private static CourseCategoryDto MapToDto(CourseCategory category) => new()
    {
        Id = category.Id,
        Name = category.Name,
        Code = category.Code,
        OrderNumber = category.OrderNumber
    };
}
