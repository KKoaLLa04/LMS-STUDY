using Backend.Authorization;
using Backend.Common;
using Backend.DTOs;
using Backend.Models;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

/// <summary>[Admin/Teacher] Quản lý kho Quiz dùng chung — độc lập với khóa học, có thể được gắn
/// vào nhiều bài học (Lesson.QuizId) hoặc học viên làm trực tiếp qua trang "Quiz chung". Teacher
/// cần quyền module Quizzes (xem RequireTeacherPermission).</summary>
[ApiController]
[Route("api/quizzes")]
[Authorize(Roles = "Admin,Teacher")]
public class QuizzesController : ControllerBase
{
    private readonly IQuizLibraryService _quizLibraryService;
    private readonly IQuizQuestionService _quizQuestionService;
    private readonly IQuizImportService _quizImportService;

    public QuizzesController(
        IQuizLibraryService quizLibraryService,
        IQuizQuestionService quizQuestionService,
        IQuizImportService quizImportService)
    {
        _quizLibraryService = quizLibraryService;
        _quizQuestionService = quizQuestionService;
        _quizImportService = quizImportService;
    }

    [HttpGet]
    [RequireTeacherPermission(PermissionModule.Quizzes, PermissionAction.View)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _quizLibraryService.GetAllAsync();
        return StatusCode(result.HttpStatusCode, result);
    }

    [HttpGet("{id:int}")]
    [RequireTeacherPermission(PermissionModule.Quizzes, PermissionAction.View)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _quizLibraryService.GetByIdAsync(id);
        return StatusCode(result.HttpStatusCode, result);
    }

    [HttpPost]
    [RequireTeacherPermission(PermissionModule.Quizzes, PermissionAction.Create)]
    public async Task<IActionResult> Create([FromBody] CreateQuizDto dto)
    {
        var result = await _quizLibraryService.CreateAsync(dto);
        return StatusCode(result.HttpStatusCode, result);
    }

    [HttpPut("{id:int}")]
    [RequireTeacherPermission(PermissionModule.Quizzes, PermissionAction.Update)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateQuizDto dto)
    {
        var result = await _quizLibraryService.UpdateAsync(id, dto);
        return StatusCode(result.HttpStatusCode, result);
    }

    [HttpDelete("{id:int}")]
    [RequireTeacherPermission(PermissionModule.Quizzes, PermissionAction.Delete)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _quizLibraryService.DeleteAsync(id);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>[Admin/Teacher] Lấy danh sách câu hỏi của quiz (kèm đáp án đúng)</summary>
    [HttpGet("{id:int}/questions")]
    [RequireTeacherPermission(PermissionModule.Quizzes, PermissionAction.View)]
    public async Task<IActionResult> GetQuestions(int id)
    {
        var result = await _quizQuestionService.GetForAdminAsync(id);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>[Admin/Teacher] Thay thế toàn bộ câu hỏi của quiz</summary>
    [HttpPut("{id:int}/questions")]
    [RequireTeacherPermission(PermissionModule.Quizzes, PermissionAction.Update)]
    public async Task<IActionResult> ReplaceQuestions(int id, [FromBody] ReplaceQuizQuestionsDto dto)
    {
        var result = await _quizQuestionService.ReplaceQuestionsAsync(id, dto);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>[Admin/Teacher] Import câu hỏi từ file Excel theo mẫu — chỉ parse, không lưu DB.
    /// Kết quả được merge vào form rồi lưu qua PUT {id}/questions như bình thường.</summary>
    [HttpPost("import-excel")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    [RequireTeacherPermission(PermissionModule.Quizzes, PermissionAction.Create)]
    public async Task<IActionResult> ImportExcel(IFormFile file)
    {
        if (file.Length == 0)
            return StatusCode(400, ApiResponse<object?>.BadRequest("File trống"));

        await using var stream = file.OpenReadStream();
        var result = _quizImportService.ParseExcel(stream);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>[Admin/Teacher] Tải file Excel mẫu để điền câu hỏi đúng định dạng import</summary>
    [HttpGet("import-template")]
    [RequireTeacherPermission(PermissionModule.Quizzes, PermissionAction.View)]
    public IActionResult DownloadTemplate()
    {
        var bytes = _quizImportService.BuildTemplate();
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "mau-import-quiz.xlsx");
    }
}
