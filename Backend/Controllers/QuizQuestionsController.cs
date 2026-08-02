using Backend.Authorization;
using Backend.Common;
using Backend.Data;
using Backend.DTOs;
using Backend.Models;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

// Lớp mỏng cho luồng soạn câu hỏi quiz gắn trong một bài học cụ thể (course-wizard) — tra
// Lesson.QuizId rồi ủy quyền cho IQuizQuestionService (đã thao tác trực tiếp trên Quiz dùng
// chung). Soạn câu hỏi cho quiz đứng độc lập thì dùng QuizzesController (api/quizzes/{id}/questions).
[ApiController]
[Route("api/lessons/{lessonId:int}/quiz-questions")]
[Authorize(Roles = "Admin,Teacher")]
public class QuizQuestionsController : ControllerBase
{
    private readonly IQuizQuestionService _quizQuestionService;
    private readonly AppDbContext _context;

    public QuizQuestionsController(IQuizQuestionService quizQuestionService, AppDbContext context)
    {
        _quizQuestionService = quizQuestionService;
        _context = context;
    }

    private async Task<(Lesson? Lesson, IActionResult? Error)> ResolveQuizLessonAsync(int lessonId)
    {
        var lesson = await _context.Lessons.FirstOrDefaultAsync(l => l.Id == lessonId);

        if (lesson == null)
            return (null, StatusCode(404, ApiResponse<object?>.NotFound("Không tìm thấy bài học")));

        if (lesson.LessonType != LessonType.Quiz || lesson.QuizId == null)
            return (null, StatusCode(400, ApiResponse<object?>.BadRequest("Bài học này không phải dạng Quiz")));

        return (lesson, null);
    }

    /// <summary>[Admin/Teacher] Lấy danh sách câu hỏi quiz của một bài học (kèm đáp án đúng)</summary>
    [HttpGet]
    [RequireTeacherPermission(PermissionModule.Quizzes, PermissionAction.View)]
    public async Task<IActionResult> GetQuestions(int lessonId)
    {
        var (lesson, error) = await ResolveQuizLessonAsync(lessonId);
        if (error != null) return error;

        var result = await _quizQuestionService.GetForAdminAsync(lesson!.QuizId!.Value);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>[Admin/Teacher] Thay thế toàn bộ câu hỏi quiz của một bài học</summary>
    [HttpPut]
    [RequireTeacherPermission(PermissionModule.Quizzes, PermissionAction.Update)]
    public async Task<IActionResult> ReplaceQuestions(int lessonId, [FromBody] ReplaceQuizQuestionsDto dto)
    {
        var (lesson, error) = await ResolveQuizLessonAsync(lessonId);
        if (error != null) return error;

        var result = await _quizQuestionService.ReplaceQuestionsAsync(lesson!.QuizId!.Value, dto);
        return StatusCode(result.HttpStatusCode, result);
    }
}
