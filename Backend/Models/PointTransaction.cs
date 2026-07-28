using System.ComponentModel.DataAnnotations;

namespace Backend.Models;

public enum PointSourceType
{
    QuizCompleted,
    LessonCompleted,
    VideoWatched,
    DiscussionPost,
    LoginStreak,
    HomeworkStreak,
    AchievementUnlocked
}

// Sổ cái điểm: mỗi hành động sinh điểm là một dòng ghi (append-only), thay vì cộng dồn
// trực tiếp vào một cột tổng điểm trên User. Nhờ vậy có thể tính lại bảng xếp hạng theo
// bất kỳ khoảng thời gian nào (tuần/tháng/toàn thời gian) chỉ bằng SUM(Points) có lọc CreatedAt,
// và giữ được lịch sử/nguồn gốc của từng điểm để truy vết khi cần.
public class PointTransaction
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    // Khóa học mà điểm này gắn với — null nghĩa là điểm không thuộc khóa học cụ thể
    // (vd điểm chuỗi đăng nhập), vẫn được cộng vào bảng xếp hạng toàn hệ thống.
    public int? CourseId { get; set; }

    public int Points { get; set; }

    public PointSourceType SourceType { get; set; }

    // Trỏ tới bản ghi nguồn gốc (LessonId, DiscussionPostId, AchievementId...) tùy SourceType.
    public int? SourceRefId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
