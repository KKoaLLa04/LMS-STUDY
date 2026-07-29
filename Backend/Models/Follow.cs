using System.ComponentModel.DataAnnotations;

namespace Backend.Models;

// Quan hệ theo dõi giữa 2 user — nguồn dữ liệu cho điều kiện huy hiệu FollowerCount
// (AchievementConditionType.FollowerCount, xem AchievementEvaluationService).
public class Follow
{
    [Key]
    public int Id { get; set; }

    public int FollowerId { get; set; }
    public User Follower { get; set; } = null!;

    public int FollowingId { get; set; }
    public User Following { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
