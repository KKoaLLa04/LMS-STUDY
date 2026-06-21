using Backend.Common;
using Backend.DTOs;

namespace Backend.Services.Interfaces;

public interface ISectionService
{
    Task<ApiResponse<SectionDetailDto>> CreateSectionAsync(CreateSectionDto dto);
    Task<ApiResponse<SectionDetailDto>> UpdateSectionAsync(int id, UpdateSectionDto dto);
    Task<ApiResponse<object?>> DeleteSectionAsync(int id);
}
