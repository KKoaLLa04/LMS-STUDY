using System.ComponentModel.DataAnnotations;

namespace Backend.Models;

// Lưu kết quả mỗi lần học sinh làm một bài học dạng Quiz (Lesson.LessonType = Quiz).
// Chưa có ngân hàng câu hỏi/chấm điểm ở backend — điểm số (ScorePercent) do luồng làm bài
// (sẽ xây dựng sau) tính toán và gửi lên; bảng này chỉ là nơi lưu kết quả để tính điểm thưởng.
public class QuizAttempt
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int LessonId { get; set; }
    public Lesson Lesson { get; set; } = null!;

    [Range(0, 100)]
    public int ScorePercent { get; set; }

    public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;
}
