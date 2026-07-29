using System.Security.Claims;
using Backend.Common;
using Backend.Data;
using Backend.DTOs;
using Backend.Models;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

// Lớp mỏng cho luồng làm quiz gắn trong một bài học cụ thể của khóa học — tra Lesson.QuizId rồi
// ủy quyền cho IQuizService/IQuizQuestionService (đã thao tác trực tiếp trên Quiz dùng chung).
// Muốn làm quiz đứng độc lập (không qua bài học nào) thì dùng StudentQuizzesController.
[ApiController]
[Route("api/lessons/{lessonId:int}/quiz")]
[Authorize]
public class QuizController : ControllerBase
{
    private readonly IQuizService _quizService;
    private readonly IQuizQuestionService _quizQuestionService;
    private readonly AppDbContext _context;

    public QuizController(IQuizService quizService, IQuizQuestionService quizQuestionService, AppDbContext context)
    {
        _quizService = quizService;
        _quizQuestionService = quizQuestionService;
        _context = context;
    }

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private async Task<(Lesson? Lesson, IActionResult? Error)> ResolveQuizLessonAsync(int lessonId)
    {
        var lesson = await _context.Lessons.Include(l => l.Section)
            .FirstOrDefaultAsync(l => l.Id == lessonId);

        if (lesson == null)
            return (null, StatusCode(404, ApiResponse<object?>.NotFound("Không tìm thấy bài học")));

        if (lesson.LessonType != LessonType.Quiz || lesson.QuizId == null)
            return (null, StatusCode(400, ApiResponse<object?>.BadRequest("Bài học này không phải dạng Quiz")));

        return (lesson, null);
    }

    /// <summary>[User] Lấy danh sách câu hỏi để làm bài (KHÔNG có đáp án đúng)</summary>
    [HttpGet("questions")]
    public async Task<IActionResult> GetQuestions(int lessonId)
    {
        var (lesson, error) = await ResolveQuizLessonAsync(lessonId);
        if (error != null) return error;

        var result = await _quizQuestionService.GetForStudentAsync(lesson!.QuizId!.Value);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>[User] Nộp kết quả làm bài quiz của một bài học</summary>
    [HttpPost("attempts")]
    public async Task<IActionResult> SubmitAttempt(int lessonId, [FromBody] SubmitQuizAttemptDto dto)
    {
        var (lesson, error) = await ResolveQuizLessonAsync(lessonId);
        if (error != null) return error;

        var result = await _quizService.SubmitAttemptAsync(CurrentUserId, lesson!.QuizId!.Value, dto, lesson.Section.CourseId);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>[User] Lịch sử các lần làm quiz của bản thân cho bài học này</summary>
    [HttpGet("attempts")]
    public async Task<IActionResult> GetMyAttempts(int lessonId)
    {
        var (lesson, error) = await ResolveQuizLessonAsync(lessonId);
        if (error != null) return error;

        var result = await _quizService.GetMyAttemptsAsync(CurrentUserId, lesson!.QuizId!.Value);
        return StatusCode(result.HttpStatusCode, result);
    }
}
