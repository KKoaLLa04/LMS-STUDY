namespace Backend.DTOs;

// Dùng cho các trang quản trị độc lập "Tài liệu" / "Quiz" — liệt kê bài học theo loại
// (Document/Quiz) trên toàn bộ khóa học, kèm tên khóa học/chương để hiển thị mà không cần
// mở từng khóa học riêng lẻ như trong course-wizard-modal.
public class LessonAdminListItemDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string LessonType { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string? VideoUrl { get; set; }
    public string? DocumentUrl { get; set; }
    public int Position { get; set; }
    public int DurationMinutes { get; set; }

    public int SectionId { get; set; }
    public string SectionTitle { get; set; } = string.Empty;
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;

    // Chỉ có ý nghĩa khi LessonType == "Quiz".
    public int QuestionCount { get; set; }
}
