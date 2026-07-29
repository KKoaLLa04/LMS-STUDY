using System.ComponentModel.DataAnnotations;

namespace Backend.Models;

public class QuizOption
{
    [Key]
    public int Id { get; set; }

    public int QuestionId { get; set; }
    public QuizQuestion Question { get; set; } = null!;

    [Required]
    public string Text { get; set; } = string.Empty;

    // Không bao giờ được lộ ra cho client học sinh trước khi nộp bài — xem
    // QuizQuestionsController (Admin, có IsCorrect) vs QuizController.GetQuestions (ẩn IsCorrect).
    public bool IsCorrect { get; set; }

    public int OrderNumber { get; set; }
}
