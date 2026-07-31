using System.ComponentModel.DataAnnotations;

namespace Backend.Models;

// Bảng nối User-Achievement: nguồn dữ liệu duy nhất cho trạng thái mở khóa huy hiệu theo
// từng học sinh (Achievement/catalog không còn cờ IsUnlocked/ProgressPercent chung nữa).
public class UserAchievement
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int AchievementId { get; set; }
    public Achievement Achievement { get; set; } = null!;

    public DateTime UnlockedAt { get; set; } = DateTime.UtcNow;
}
