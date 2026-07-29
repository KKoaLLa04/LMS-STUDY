using System.ComponentModel.DataAnnotations;

namespace Backend.Models;

// Lưu kết quả mỗi lần học sinh làm một Quiz dùng chung (đứng độc lập hoặc được gắn vào một
// Lesson qua Lesson.QuizId). QuizId để nullable — một số attempt lịch sử (trước khi tách Quiz
// khỏi Lesson) có thể không xác định lại được Quiz gốc trong quá trình backfill dữ liệu, và ta
// không muốn xóa mất lịch sử vì lý do đó.
public class QuizAttempt
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int? QuizId { get; set; }
    public Quiz? Quiz { get; set; }

    [Range(0, 100)]
    public int ScorePercent { get; set; }

    public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;
}
