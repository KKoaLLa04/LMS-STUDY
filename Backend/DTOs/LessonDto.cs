using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs;

public class CreateLessonDto
{
    [Range(1, int.MaxValue, ErrorMessage = "SectionId không hợp lệ")]
    public int SectionId { get; set; }

    [Required(ErrorMessage = "Tiêu đề không được để trống")]
    [MaxLength(255, ErrorMessage = "Tiêu đề không vượt quá 255 ký tự")]
    public string Title { get; set; } = string.Empty;

    public string? Content { get; set; }

    [MaxLength(500, ErrorMessage = "VideoUrl không vượt quá 500 ký tự")]
    public string? VideoUrl { get; set; }

    public string LessonType { get; set; } = "Video";

    [Range(0, int.MaxValue, ErrorMessage = "Position không được âm")]
    public int Position { get; set; }
}

public class UpdateLessonDto
{
    [Required(ErrorMessage = "Tiêu đề không được để trống")]
    [MaxLength(255, ErrorMessage = "Tiêu đề không vượt quá 255 ký tự")]
    public string Title { get; set; } = string.Empty;

    public string? Content { get; set; }

    [MaxLength(500, ErrorMessage = "VideoUrl không vượt quá 500 ký tự")]
    public string? VideoUrl { get; set; }

    public string LessonType { get; set; } = "Video";

    [Range(0, int.MaxValue, ErrorMessage = "Position không được âm")]
    public int Position { get; set; }
}

public class LessonResponseDto
{
    public int Id { get; set; }
    public int SectionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string? VideoUrl { get; set; }
    public string LessonType { get; set; } = string.Empty;
    public int Position { get; set; }
}
