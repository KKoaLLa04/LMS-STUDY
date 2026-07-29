using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

/// <summary>[User] Xem Tài liệu chung trực tiếp, không cần qua khóa học nào. Mở cho mọi
/// người dùng đã đăng nhập — cùng chính sách với QuizController hiện tại (không có
/// enrollment-gating trong hệ thống).</summary>
[ApiController]
[Route("api/student/documents")]
[Authorize]
public class StudentDocumentsController : ControllerBase
{
    private readonly IDocumentService _documentService;

    public StudentDocumentsController(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _documentService.GetAllForStudentAsync();
        return StatusCode(result.HttpStatusCode, result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _documentService.GetForStudentAsync(id);
        return StatusCode(result.HttpStatusCode, result);
    }
}
