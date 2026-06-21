using System.ComponentModel.DataAnnotations;

namespace Backend.Models;

public enum LessonType
{
    Video,
    Document
}

public class Lesson
{
    [Key]
    public int Id { get; set; }

    public int SectionId { get; set; }

    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    public string? Content { get; set; }

    [MaxLength(500)]
    public string? VideoUrl { get; set; }

    public LessonType LessonType { get; set; } = LessonType.Video;

    public int Position { get; set; }

    public Section Section { get; set; } = null!;
}
