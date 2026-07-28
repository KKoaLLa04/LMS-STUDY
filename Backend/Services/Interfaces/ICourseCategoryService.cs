using Backend.Common;
using Backend.DTOs;

namespace Backend.Services.Interfaces;

public interface ICourseCategoryService
{
    Task<ApiResponse<List<CourseCategoryDto>>> GetAllAsync();
    Task<ApiResponse<CourseCategoryDto>> GetByIdAsync(int id);
    Task<ApiResponse<CourseCategoryDto>> CreateAsync(CreateCourseCategoryDto dto);
    Task<ApiResponse<CourseCategoryDto>> UpdateAsync(int id, UpdateCourseCategoryDto dto);
    Task<ApiResponse<object?>> DeleteAsync(int id);
}
