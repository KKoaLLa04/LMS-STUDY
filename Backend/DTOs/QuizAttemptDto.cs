namespace Backend.DTOs;

public class QuizAnswerSubmissionDto
{
    public int QuestionId { get; set; }
    public List<int> SelectedOptionIds { get; set; } = new();
}

public class SubmitQuizAttemptDto
{
    public List<QuizAnswerSubmissionDto> Answers { get; set; } = new();
}

public class QuizAttemptDto
{
    public int Id { get; set; }
    public int? QuizId { get; set; }
    public int ScorePercent { get; set; }
    public DateTime AttemptedAt { get; set; }
}

public class QuizQuestionResultDto
{
    public int QuestionId { get; set; }
    public bool IsCorrect { get; set; }
    public List<int> CorrectOptionIds { get; set; } = new();
    public List<int> SelectedOptionIds { get; set; } = new();
}

public class QuizAttemptResultDto
{
    public QuizAttemptDto Attempt { get; set; } = null!;
    public int BestScorePercent { get; set; }
    public int PointsAwarded { get; set; }
    public List<QuizQuestionResultDto> QuestionResults { get; set; } = new();
}
