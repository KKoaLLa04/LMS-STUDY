using System.ComponentModel.DataAnnotations;
using Backend.Common;

namespace Backend.Models;

public enum AchievementCategory
{
    HocTap,
    ChuyenCan,
    TuongTac
}

public class Achievement : ISoftDelete
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    public AchievementCategory Category { get; set; }

    // Tên icon phía client (app-oc-icon), vd. "book-open", "flame", "trophy".
    [Required]
    [MaxLength(50)]
    public string IconKey { get; set; } = string.Empty;

    public int OrderNumber { get; set; }

    // Chưa có khái niệm học sinh đăng nhập/theo dõi tiến độ riêng ở khu vực client
    // (Phase 2/3 của lộ trình), nên trạng thái mở khóa/tiến độ được quản trị viên
    // gán trực tiếp trên huy hiệu thay vì tính theo từng học sinh.
    public bool IsUnlocked { get; set; }

    [Range(0, 100)]
    public int ProgressPercent { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}
