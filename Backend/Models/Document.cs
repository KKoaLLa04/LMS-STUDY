using System.ComponentModel.DataAnnotations;
using Backend.Common;

namespace Backend.Models;

// Tài liệu dùng chung — không bắt buộc thuộc bài học/khóa học nào. Có thể được nhiều Lesson
// (Lesson.DocumentId) tham chiếu tới cùng một Document, và học viên cũng có thể xem trực tiếp
// qua trang "Tài liệu chung" mà không cần qua khóa học. Nội dung hybrid: Content (text soạn
// trực tiếp) và FileUrl (upload) độc lập với nhau, có thể có một hoặc cả hai.
public class Document : ISoftDelete
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    public string? Content { get; set; }

    [MaxLength(500)]
    public string? FileUrl { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}
