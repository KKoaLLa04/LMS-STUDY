using System.ComponentModel.DataAnnotations;

namespace Backend.Models;

// Lượt thích trên một bài viết/trả lời thảo luận — nguồn dữ liệu cho điều kiện huy hiệu
// ReceivedLikeCount (AchievementConditionType.ReceivedLikeCount, xem AchievementEvaluationService).
public class DiscussionPostLike
{
    [Key]
    public int Id { get; set; }

    public int PostId { get; set; }
    public DiscussionPost Post { get; set; } = null!;

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
