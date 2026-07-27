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

    public UploadsController(IUploadService uploadService)
    {
        _uploadService = uploadService;
    }

    /// <summary>
    /// [Admin] Tải video bài học lên server
    /// </summary>
    [HttpPost("video")]
    [RequestSizeLimit(500 * 1024 * 1024)]
    public async Task<IActionResult> UploadVideo(IFormFile file)
    {
        var result = await _uploadService.SaveVideoAsync(file, Request.Scheme, Request.Host.Value);
        return StatusCode(result.HttpStatusCode, result);
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
}
