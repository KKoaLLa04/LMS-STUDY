using System.ComponentModel.DataAnnotations;

namespace Backend.Models;

public class DiscussionPost
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;

    [MaxLength(100)]
    public string AuthorName { get; set; } = string.Empty;

    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;

    public int? ParentPostId { get; set; }
    public DiscussionPost? ParentPost { get; set; }
    public ICollection<DiscussionPost> Replies { get; set; } = new List<DiscussionPost>();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
