using System.Security.Claims;
using Backend.DTOs;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

/// <summary>[User] Làm Quiz chung trực tiếp, không cần qua khóa học/bài học nào. Mở cho mọi
/// người dùng đã đăng nhập — cùng chính sách với QuizController hiện tại (không có
/// enrollment-gating trong hệ thống). Lịch sử làm bài (QuizAttempt) dùng chung với luồng làm
/// quiz qua bài học trong khóa học vì cùng khóa theo QuizId — điểm "cải thiện" và điểm thưởng
/// vì vậy được tính chung, không tách riêng theo course.</summary>
[ApiController]
[Route("api/student/quizzes")]
[Authorize]
public class StudentQuizzesController : ControllerBase
{
    private readonly IQuizLibraryService _quizLibraryService;
    private readonly IQuizQuestionService _quizQuestionService;
    private readonly IQuizService _quizService;

    public StudentQuizzesController(
        IQuizLibraryService quizLibraryService,
        IQuizQuestionService quizQuestionService,
        IQuizService quizService)
    {
        _quizLibraryService = quizLibraryService;
        _quizQuestionService = quizQuestionService;
        _quizService = quizService;
    }

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _quizLibraryService.GetAllForStudentAsync();
        return StatusCode(result.HttpStatusCode, result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _quizLibraryService.GetForStudentAsync(id);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>[User] Lấy danh sách câu hỏi để làm bài (KHÔNG có đáp án đúng)</summary>
    [HttpGet("{id:int}/questions")]
    public async Task<IActionResult> GetQuestions(int id)
    {
        var result = await _quizQuestionService.GetForStudentAsync(id);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>[User] Nộp kết quả làm bài quiz</summary>
    [HttpPost("{id:int}/attempts")]
    public async Task<IActionResult> SubmitAttempt(int id, [FromBody] SubmitQuizAttemptDto dto)
    {
        var result = await _quizService.SubmitAttemptAsync(CurrentUserId, id, dto);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>[User] Lịch sử các lần làm quiz của bản thân</summary>
    [HttpGet("{id:int}/attempts")]
    public async Task<IActionResult> GetMyAttempts(int id)
    {
        var result = await _quizService.GetMyAttemptsAsync(CurrentUserId, id);
        return StatusCode(result.HttpStatusCode, result);
    }
}
