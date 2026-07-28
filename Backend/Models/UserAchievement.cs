using System.ComponentModel.DataAnnotations;

namespace Backend.Models;

// Bảng nối User-Achievement: ghi nhận huy hiệu nào đã mở khóa cho học sinh nào.
// Achievement (catalog) vẫn giữ IsUnlocked/ProgressPercent cũ dùng cho màn quản trị chung,
// còn bảng này là nguồn dữ liệu thật cho trạng thái mở khóa theo từng học sinh.
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
