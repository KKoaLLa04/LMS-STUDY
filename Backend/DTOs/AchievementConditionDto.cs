using System.ComponentModel.DataAnnotations;
using Backend.Models;

namespace Backend.DTOs;

public class CreateAchievementConditionDto
{
    [Required(ErrorMessage = "Loại điều kiện không được để trống")]
    public AchievementConditionType ConditionType { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Giá trị mục tiêu phải lớn hơn 0")]
    public int TargetValue { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Nhóm điều kiện không được âm")]
    public int LogicGroup { get; set; }
}

public class AchievementConditionDto
{
    public int Id { get; set; }
    public AchievementConditionType ConditionType { get; set; }
    public int TargetValue { get; set; }
    public int LogicGroup { get; set; }
}

// Metadata mô tả một loại điều kiện — dùng để render dropdown ở Frontend
// (xem GET /api/achievement-conditions/types).
public class AchievementConditionTypeDto
{
    public string Code { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string Placeholder { get; set; } = string.Empty;
}
