using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs;

public class CreateAchievementDto
{
    [Required(ErrorMessage = "Tên huy hiệu không được để trống")]
    [MaxLength(255, ErrorMessage = "Tên huy hiệu không vượt quá 255 ký tự")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mô tả không được để trống")]
    [MaxLength(500, ErrorMessage = "Mô tả không vượt quá 500 ký tự")]
    public string Description { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Nhóm huy hiệu không được để trống")]
    public int GroupId { get; set; }

    [Required(ErrorMessage = "Icon không được để trống")]
    [MaxLength(50, ErrorMessage = "Icon không vượt quá 50 ký tự")]
    public string IconKey { get; set; } = string.Empty;

    [Range(0, int.MaxValue, ErrorMessage = "Thứ tự không được âm")]
    public int OrderNumber { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Điểm thưởng không được âm")]
    public int Points { get; set; } = 50;

    [MinLength(1, ErrorMessage = "Cần có ít nhất một điều kiện đạt được")]
    public List<CreateAchievementConditionDto> Conditions { get; set; } = new();
}

public class UpdateAchievementDto
{
    [Required(ErrorMessage = "Tên huy hiệu không được để trống")]
    [MaxLength(255, ErrorMessage = "Tên huy hiệu không vượt quá 255 ký tự")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mô tả không được để trống")]
    [MaxLength(500, ErrorMessage = "Mô tả không vượt quá 500 ký tự")]
    public string Description { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Nhóm huy hiệu không được để trống")]
    public int GroupId { get; set; }

    [Required(ErrorMessage = "Icon không được để trống")]
    [MaxLength(50, ErrorMessage = "Icon không vượt quá 50 ký tự")]
    public string IconKey { get; set; } = string.Empty;

    [Range(0, int.MaxValue, ErrorMessage = "Thứ tự không được âm")]
    public int OrderNumber { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Điểm thưởng không được âm")]
    public int Points { get; set; } = 50;

    [MinLength(1, ErrorMessage = "Cần có ít nhất một điều kiện đạt được")]
    public List<CreateAchievementConditionDto> Conditions { get; set; } = new();
}

public class AchievementDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;

    // Giữ tương thích ngược cho Frontend/src/app/clients (đọc field "category" dạng chuỗi ổn
    // định — xem AchievementGroup.Code) — trang admin dùng GroupId/GroupName ở trên thay thế.
    public string Category { get; set; } = string.Empty;

    public string IconKey { get; set; } = string.Empty;
    public int OrderNumber { get; set; }
    public int Points { get; set; }
    public List<AchievementConditionDto> Conditions { get; set; } = new();
}

// Trạng thái mở khóa huy hiệu thật của MỘT học sinh cụ thể, qua bảng UserAchievement
// (nguồn dữ liệu duy nhất — Achievement không còn cờ IsUnlocked/ProgressPercent chung nữa).
public class MyAchievementDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string IconKey { get; set; } = string.Empty;
    public int OrderNumber { get; set; }
    public int Points { get; set; }
    public bool UnlockedByMe { get; set; }
    public DateTime? UnlockedAt { get; set; }
}
