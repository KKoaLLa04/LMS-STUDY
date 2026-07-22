using Backend.Common;
using Backend.DTOs;
using Microsoft.AspNetCore.Http;

namespace Backend.Services.Interfaces;

public interface IUploadService
{
    Task<ApiResponse<UploadResultDto>> SaveVideoAsync(IFormFile file, string requestScheme, string requestHost);
}
