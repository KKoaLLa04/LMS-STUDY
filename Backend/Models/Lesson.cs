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

    // File tài liệu (PDF/Word/PowerPoint...) đính kèm cho bài học dạng Document —
    // độc lập với Content (nội dung soạn trực tiếp dạng rich text), một bài học Document
    // có thể có cả hai hoặc chỉ một trong hai.
    // Lưu ý: Content/DocumentUrl ở đây là các field cũ, chỉ còn dùng cho dữ liệu tạo trước khi
    // có kho Document/Quiz dùng chung — bài học Document/Quiz tạo mới luôn dùng DocumentId/QuizId.
    [MaxLength(500)]
    public string? DocumentUrl { get; set; }

    public LessonType LessonType { get; set; } = LessonType.Video;

    public int Position { get; set; }

    public int DurationMinutes { get; set; }

    // Tài liệu dùng chung được gắn cho bài học dạng Document (nullable — nhiều Lesson có thể
    // cùng trỏ tới một Document; xóa Document bị chặn nếu còn Lesson tham chiếu, xem DocumentService).
    public int? DocumentId { get; set; }
    public Document? Document { get; set; }

    // Quiz dùng chung được gắn cho bài học dạng Quiz (nullable — nhiều Lesson có thể cùng trỏ
    // tới một Quiz; xóa Quiz bị chặn nếu còn Lesson tham chiếu, xem QuizLibraryService).
    public int? QuizId { get; set; }
    public Quiz? Quiz { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    public Section Section { get; set; } = null!;
}
