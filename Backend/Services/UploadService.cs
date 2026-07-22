using Backend.Common;
using Backend.DTOs;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Backend.Services;

public class UploadService : IUploadService
{
    private static readonly string[] AllowedExtensions = { ".mp4", ".webm", ".mov", ".avi", ".mkv" };
    private const long MaxFileSize = 500 * 1024 * 1024; // 500MB

    private readonly IWebHostEnvironment _env;
    private readonly ILogger<UploadService> _logger;

    public UploadService(IWebHostEnvironment env, ILogger<UploadService> logger)
    {
        _env = env;
        _logger = logger;
    }

    public async Task<ApiResponse<UploadResultDto>> SaveVideoAsync(IFormFile file, string requestScheme, string requestHost)
    {
        try
        {
            if (file == null || file.Length == 0)
                return ApiResponse<UploadResultDto>.BadRequest("Vui lòng chọn file video");

            if (file.Length > MaxFileSize)
                return ApiResponse<UploadResultDto>.BadRequest("File video không được vượt quá 500MB");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
                return ApiResponse<UploadResultDto>.BadRequest(
                    $"Định dạng file không hợp lệ. Chỉ chấp nhận: {string.Join(", ", AllowedExtensions)}");

            var webRootPath = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
            var uploadsDir = Path.Combine(webRootPath, "uploads", "videos");
            Directory.CreateDirectory(uploadsDir);

            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsDir, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var url = $"{requestScheme}://{requestHost}/uploads/videos/{fileName}";
            return ApiResponse<UploadResultDto>.Ok(new UploadResultDto { Url = url }, "Tải video lên thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tải video lên");
            return ApiResponse<UploadResultDto>.Error("Đã xảy ra lỗi khi tải video lên");
        }
    }
}
