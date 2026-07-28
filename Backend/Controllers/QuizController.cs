using System.Security.Claims;
using Backend.DTOs;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/lessons/{lessonId:int}/quiz")]
[Authorize]
public class QuizController : ControllerBase
{
    private readonly IQuizService _quizService;

    public QuizController(IQuizService quizService)
    {
        _quizService = quizService;
    }

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>[User] Nộp kết quả làm bài quiz của một bài học</summary>
    [HttpPost("attempts")]
    public async Task<IActionResult> SubmitAttempt(int lessonId, [FromBody] SubmitQuizAttemptDto dto)
    {
        var result = await _quizService.SubmitAttemptAsync(CurrentUserId, lessonId, dto);
        return StatusCode(result.HttpStatusCode, result);
    }

    /// <summary>[User] Lịch sử các lần làm quiz của bản thân cho bài học này</summary>
    [HttpGet("attempts")]
    public async Task<IActionResult> GetMyAttempts(int lessonId)
    {
        var result = await _quizService.GetMyAttemptsAsync(CurrentUserId, lessonId);
        return StatusCode(result.HttpStatusCode, result);
    }
}
