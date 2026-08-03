using Backend.Common;
using Backend.DTOs;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UploadsController : ControllerBase
{
    private readonly IUploadService _uploadService;
    private readonly IR2StorageService _r2StorageService;

    public UploadsController(IUploadService uploadService, IR2StorageService r2StorageService)
    {
        _uploadService = uploadService;
        _r2StorageService = r2StorageService;
    }

    /// <summary>
    /// [Admin] Xin presigned URL để upload video thẳng lên Cloudflare R2 (browser tự PUT, không qua server)
    /// </summary>
    [HttpPost("video/presign")]
    public async Task<IActionResult> PresignVideo([FromBody] PresignVideoRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.FileName))
            return StatusCode(400, ApiResponse<PresignResultDto>.BadRequest("Vui lòng cung cấp tên file"));

        try
        {
            var (uploadUrl, key) = await _r2StorageService.GeneratePresignedUploadUrlAsync(dto.FileName, dto.ContentType);
            var result = ApiResponse<PresignResultDto>.Ok(
                new PresignResultDto { UploadUrl = uploadUrl, Key = key }, "Tạo URL upload thành công");
            return StatusCode(result.HttpStatusCode, result);
        }
        catch (ArgumentException ex)
        {
            var result = ApiResponse<PresignResultDto>.BadRequest(ex.Message);
            return StatusCode(result.HttpStatusCode, result);
        }
    }

    /// <summary>
    /// [Admin] Tải ảnh bìa khóa học lên server
    /// </summary>
    [HttpPost("image")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        var result = await _uploadService.SaveImageAsync(file, Request.Scheme, Request.Host.Value);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>
    /// [Admin] Tải tài liệu (PDF/Word/PowerPoint/Excel) lên server cho bài học dạng Document
    /// </summary>
    [HttpPost("document")]
    [RequestSizeLimit(50 * 1024 * 1024)]
    public async Task<IActionResult> UploadDocument(IFormFile file)
    {
        var result = await _uploadService.SaveDocumentAsync(file, Request.Scheme, Request.Host.Value);
        return StatusCode(result.HttpStatusCode, result);
    }
}
