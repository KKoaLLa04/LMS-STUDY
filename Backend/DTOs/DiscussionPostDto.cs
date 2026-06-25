using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs;

public class CreateDiscussionPostDto
{
    [Required(ErrorMessage = "Tiêu đề không được để trống")]
    [MaxLength(255, ErrorMessage = "Tiêu đề không vượt quá 255 ký tự")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nội dung không được để trống")]
    public string Content { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tên tác giả không được để trống")]
    [MaxLength(100)]
    public string AuthorName { get; set; } = string.Empty;

    [Required]
    public int CourseId { get; set; }

    public int? ParentPostId { get; set; }
}

public class UpdateDiscussionPostDto
{
    [Required(ErrorMessage = "Tiêu đề không được để trống")]
    [MaxLength(255, ErrorMessage = "Tiêu đề không vượt quá 255 ký tự")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nội dung không được để trống")]
    public string Content { get; set; } = string.Empty;
}

public class DiscussionPostResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public int? ParentPostId { get; set; }
    public List<DiscussionPostResponseDto> Replies { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class DiscussionPostListItemDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public int ReplyCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
