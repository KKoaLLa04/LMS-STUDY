using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
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
}
