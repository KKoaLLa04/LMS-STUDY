using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs;

public class SubmitQuizAttemptDto
{
    [Range(0, 100, ErrorMessage = "Điểm số phải trong khoảng 0-100")]
    public int ScorePercent { get; set; }
}

public class QuizAttemptDto
{
    public int Id { get; set; }
    public int LessonId { get; set; }
    public int ScorePercent { get; set; }
    public DateTime AttemptedAt { get; set; }
}

public class QuizAttemptResultDto
{
    public QuizAttemptDto Attempt { get; set; } = null!;
    public int BestScorePercent { get; set; }
    public int PointsAwarded { get; set; }
}
