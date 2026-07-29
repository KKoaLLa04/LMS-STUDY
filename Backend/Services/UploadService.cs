using Backend.Common;
using Backend.DTOs;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Backend.Services;

public class UploadService : IUploadService
{
    private static readonly string[] AllowedExtensions = { ".mp4", ".webm", ".mov", ".avi", ".mkv" };
    private const long MaxFileSize = 500 * 1024 * 1024; // 500MB

    private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
    private const long MaxImageSize = 10 * 1024 * 1024; // 10MB

    private static readonly string[] AllowedDocumentExtensions =
        { ".pdf", ".doc", ".docx", ".ppt", ".pptx", ".xls", ".xlsx" };
    private const long MaxDocumentSize = 50 * 1024 * 1024; // 50MB

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

    public async Task<ApiResponse<UploadResultDto>> SaveImageAsync(IFormFile file, string requestScheme, string requestHost)
    {
        try
        {
            if (file == null || file.Length == 0)
                return ApiResponse<UploadResultDto>.BadRequest("Vui lòng chọn file ảnh");

            if (file.Length > MaxImageSize)
                return ApiResponse<UploadResultDto>.BadRequest("File ảnh không được vượt quá 10MB");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedImageExtensions.Contains(extension))
                return ApiResponse<UploadResultDto>.BadRequest(
                    $"Định dạng file không hợp lệ. Chỉ chấp nhận: {string.Join(", ", AllowedImageExtensions)}");

            var webRootPath = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
            var uploadsDir = Path.Combine(webRootPath, "uploads", "images");
            Directory.CreateDirectory(uploadsDir);

            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsDir, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var url = $"{requestScheme}://{requestHost}/uploads/images/{fileName}";
            return ApiResponse<UploadResultDto>.Ok(new UploadResultDto { Url = url }, "Tải ảnh lên thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tải ảnh lên");
            return ApiResponse<UploadResultDto>.Error("Đã xảy ra lỗi khi tải ảnh lên");
        }
    }

    public async Task<ApiResponse<UploadResultDto>> SaveDocumentAsync(IFormFile file, string requestScheme, string requestHost)
    {
        try
        {
            if (file == null || file.Length == 0)
                return ApiResponse<UploadResultDto>.BadRequest("Vui lòng chọn file tài liệu");

            if (file.Length > MaxDocumentSize)
                return ApiResponse<UploadResultDto>.BadRequest("File tài liệu không được vượt quá 50MB");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedDocumentExtensions.Contains(extension))
                return ApiResponse<UploadResultDto>.BadRequest(
                    $"Định dạng file không hợp lệ. Chỉ chấp nhận: {string.Join(", ", AllowedDocumentExtensions)}");

            var webRootPath = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
            var uploadsDir = Path.Combine(webRootPath, "uploads", "documents");
            Directory.CreateDirectory(uploadsDir);

            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsDir, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var url = $"{requestScheme}://{requestHost}/uploads/documents/{fileName}";
            return ApiResponse<UploadResultDto>.Ok(new UploadResultDto { Url = url }, "Tải tài liệu lên thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tải tài liệu lên");
            return ApiResponse<UploadResultDto>.Error("Đã xảy ra lỗi khi tải tài liệu lên");
        }
    }
}
