using Backend.DTOs;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

/// <summary>[Admin] Quản lý kho Tài liệu dùng chung — độc lập với khóa học, có thể được gắn
/// vào nhiều bài học (Lesson.DocumentId) hoặc học viên xem trực tiếp qua trang "Tài liệu chung".</summary>
[ApiController]
[Route("api/documents")]
[Authorize(Roles = "Admin")]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _documentService;

    public DocumentsController(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _documentService.GetAllAsync();
        return StatusCode(result.HttpStatusCode, result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _documentService.GetByIdAsync(id);
        return StatusCode(result.HttpStatusCode, result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDocumentDto dto)
    {
        var result = await _documentService.CreateAsync(dto);
        return StatusCode(result.HttpStatusCode, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDocumentDto dto)
    {
        var result = await _documentService.UpdateAsync(id, dto);
        return StatusCode(result.HttpStatusCode, result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _documentService.DeleteAsync(id);
        return StatusCode(result.HttpStatusCode, result);
    }
}
