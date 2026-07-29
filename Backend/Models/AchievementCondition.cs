using System.ComponentModel.DataAnnotations;

namespace Backend.Models;

public enum AchievementConditionType
{
    CompleteLessonCount,
    CompleteCourseCount,
    QuizPerfectScoreCount,
    QuizMinScorePercent,
    LoginStreakDays,
    CommentCount,
    ReceivedLikeCount,
    FollowerCount,
    RegisterBeforeHour
}

// Điều kiện đạt được của một Achievement. Nhiều dòng cùng LogicGroup phải đạt hết (AND),
// giữa các LogicGroup khác nhau chỉ cần một nhóm đạt hết là achievement được mở khóa (OR).
// Xem AchievementEvaluationService — nơi đọc bảng này để tự động trao huy hiệu.
public class AchievementCondition
{
    [Key]
    public int Id { get; set; }

    public int AchievementId { get; set; }
    public Achievement Achievement { get; set; } = null!;

    public AchievementConditionType ConditionType { get; set; }

    public int TargetValue { get; set; }

    public int LogicGroup { get; set; }
}
