using System.ComponentModel.DataAnnotations;
using Backend.Common;

namespace Backend.Models;

public enum LessonType
{
    Video,
    Document,
    Quiz
}

public class Lesson : ISoftDelete
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

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    public Section Section { get; set; } = null!;
}
