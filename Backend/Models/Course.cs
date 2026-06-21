using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

public enum CourseStatus
{
    Draft,
    Published
}

public class Course
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [MaxLength(500)]
    public string? Thumbnail { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    public CourseStatus Status { get; set; } = CourseStatus.Draft;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Section> Sections { get; set; } = new List<Section>();
}
