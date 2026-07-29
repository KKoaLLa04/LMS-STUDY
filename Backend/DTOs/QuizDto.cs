using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs;

public class CreateQuizDto
{
    [Required(ErrorMessage = "Tiêu đề không được để trống")]
    [MaxLength(255, ErrorMessage = "Tiêu đề không vượt quá 255 ký tự")]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }
}

public class UpdateQuizDto
{
    [Required(ErrorMessage = "Tiêu đề không được để trống")]
    [MaxLength(255, ErrorMessage = "Tiêu đề không vượt quá 255 ký tự")]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }
}

public class QuizResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int QuestionCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int LinkedLessonCount { get; set; }
}

// DTO gọn cho học viên xem trực tiếp ở trang "Quiz chung".
public class StudentQuizDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int QuestionCount { get; set; }
}
