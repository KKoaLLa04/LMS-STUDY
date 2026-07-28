using System.ComponentModel.DataAnnotations;

namespace Backend.Models;

// Theo dõi tiến độ xem bài học (video/tài liệu) theo từng học sinh — nguồn cho điểm
// "xem video" và điểm "hoàn thành bài học", đồng thời là tín hiệu cho chuỗi ngày học tập.
public class LessonProgress
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int LessonId { get; set; }
    public Lesson Lesson { get; set; } = null!;

    // Cao điểm đã ghi nhận (không giảm), dùng để tính điểm theo phần chênh lệch mỗi lần cập nhật
    // và chặn việc "farm" điểm bằng cách tua đi tua lại video.
    public int WatchedSeconds { get; set; }

    public bool IsCompleted { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
