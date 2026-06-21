using System.ComponentModel.DataAnnotations;

namespace Backend.Models;

public class Section
{
    [Key]
    public int Id { get; set; }

    public int CourseId { get; set; }

    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    public int Position { get; set; }

    public Course Course { get; set; } = null!;

    public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
}
