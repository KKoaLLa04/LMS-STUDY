using Backend.Common;
using Backend.DTOs;

namespace Backend.Services.Interfaces;

public interface IPlatformStatsService
{
    Task<ApiResponse<PlatformStatsDto>> GetStatsAsync();
}
