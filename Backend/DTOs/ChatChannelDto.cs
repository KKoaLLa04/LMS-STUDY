using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs;

public class CreateChatChannelDto
{
    [Required(ErrorMessage = "Tên kênh không được để trống")]
    [MaxLength(100, ErrorMessage = "Tên kênh không vượt quá 100 ký tự")]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    public int CourseId { get; set; }
}

public class UpdateChatChannelDto
{
    [Required(ErrorMessage = "Tên kênh không được để trống")]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}

public class ChatChannelResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int MessageCount { get; set; }
}

public class SendMessageDto
{
    [Required(ErrorMessage = "Nội dung tin nhắn không được để trống")]
    public string Content { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tên người gửi không được để trống")]
    [MaxLength(100)]
    public string SenderName { get; set; } = string.Empty;
}

public class ChatMessageResponseDto
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public int ChannelId { get; set; }
    public DateTime SentAt { get; set; }
}
