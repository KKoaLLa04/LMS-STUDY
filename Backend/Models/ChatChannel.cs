using System.ComponentModel.DataAnnotations;

namespace Backend.Models;

public class ChatChannel
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
}

public class ChatMessage
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Content { get; set; } = string.Empty;

    [MaxLength(100)]
    public string SenderName { get; set; } = string.Empty;

    public int ChannelId { get; set; }
    public ChatChannel Channel { get; set; } = null!;

    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}
