using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs;

public class CreateSectionDto
{
    [Range(1, int.MaxValue, ErrorMessage = "CourseId không hợp lệ")]
    public int CourseId { get; set; }

    [Required(ErrorMessage = "Tiêu đề không được để trống")]
    [MaxLength(255, ErrorMessage = "Tiêu đề không vượt quá 255 ký tự")]
    public string Title { get; set; } = string.Empty;

    [Range(0, int.MaxValue, ErrorMessage = "Position không được âm")]
    public int Position { get; set; }
}

public class UpdateSectionDto
{
    [Required(ErrorMessage = "Tiêu đề không được để trống")]
    [MaxLength(255, ErrorMessage = "Tiêu đề không vượt quá 255 ký tự")]
    public string Title { get; set; } = string.Empty;

    [Range(0, int.MaxValue, ErrorMessage = "Position không được âm")]
    public int Position { get; set; }
}

public class SectionDetailDto
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Position { get; set; }
    public List<LessonResponseDto> Lessons { get; set; } = new();
}
