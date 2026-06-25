using Backend.Common;
using Backend.DTOs;

namespace Backend.Services.VirtualClassrooms.Interfaces;

public interface IVirtualClassroomService
{
    Task<ApiResponse<PagedResultDto<VirtualClassroomResponseDto>>> GetClassroomsByCourseAsync(int courseId, int page, int pageSize);
    Task<ApiResponse<VirtualClassroomResponseDto>> GetClassroomByIdAsync(int id);
    Task<ApiResponse<VirtualClassroomResponseDto>> CreateClassroomAsync(CreateVirtualClassroomDto dto);
    Task<ApiResponse<VirtualClassroomResponseDto>> UpdateClassroomAsync(int id, UpdateVirtualClassroomDto dto);
    Task<ApiResponse<object?>> DeleteClassroomAsync(int id);
    Task<ApiResponse<VirtualClassroomResponseDto>> UpdateStatusAsync(int id, string status);
}
