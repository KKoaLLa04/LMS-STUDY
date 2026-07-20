using Backend.Common;
using Backend.DTOs;

namespace Backend.Services.Interfaces;

public interface IKhoiHocService
{
    Task<ApiResponse<List<KhoiHocDto>>> GetAllAsync();
    Task<ApiResponse<KhoiHocDto>> GetByIdAsync(int id);
    Task<ApiResponse<KhoiHocDto>> CreateAsync(CreateKhoiHocDto dto);
    Task<ApiResponse<KhoiHocDto>> UpdateAsync(int id, UpdateKhoiHocDto dto);
    Task<ApiResponse<object?>> DeleteAsync(int id);
}
