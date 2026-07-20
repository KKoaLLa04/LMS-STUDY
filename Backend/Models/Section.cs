using System.ComponentModel.DataAnnotations;
using Backend.Common;

namespace Backend.Models;

public class Section : ISoftDelete
{
    [Key]
    public int Id { get; set; }

    public int CourseId { get; set; }

    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    public int Position { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    public Course Course { get; set; } = null!;

    public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
}
