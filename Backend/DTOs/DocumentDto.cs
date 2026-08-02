using System.ComponentModel.DataAnnotations;
using Backend.Models;

namespace Backend.DTOs;

public class CreateDocumentDto
{
    [Required(ErrorMessage = "Tiêu đề không được để trống")]
    [MaxLength(255, ErrorMessage = "Tiêu đề không vượt quá 255 ký tự")]
    public string Title { get; set; } = string.Empty;

    public string? Content { get; set; }

    [MaxLength(500, ErrorMessage = "FileUrl không vượt quá 500 ký tự")]
    public string? FileUrl { get; set; }

    public DocumentStatus Status { get; set; } = DocumentStatus.SharedAndForLesson;
}

public class UpdateDocumentDto
{
    [Required(ErrorMessage = "Tiêu đề không được để trống")]
    [MaxLength(255, ErrorMessage = "Tiêu đề không vượt quá 255 ký tự")]
    public string Title { get; set; } = string.Empty;

    public string? Content { get; set; }

    [MaxLength(500, ErrorMessage = "FileUrl không vượt quá 500 ký tự")]
    public string? FileUrl { get; set; }

    public DocumentStatus Status { get; set; }
}

public class DocumentResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string? FileUrl { get; set; }
    public DocumentStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int LinkedLessonCount { get; set; }
}

// DTO gọn cho học viên xem trực tiếp ở trang "Tài liệu chung" (không cần LinkedLessonCount).
public class StudentDocumentDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string? FileUrl { get; set; }
}
