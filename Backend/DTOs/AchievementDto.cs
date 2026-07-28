using System.ComponentModel.DataAnnotations;
using Backend.Models;

namespace Backend.DTOs;

public class CreateAchievementDto
{
    [Required(ErrorMessage = "Tên huy hiệu không được để trống")]
    [MaxLength(255, ErrorMessage = "Tên huy hiệu không vượt quá 255 ký tự")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mô tả không được để trống")]
    [MaxLength(500, ErrorMessage = "Mô tả không vượt quá 500 ký tự")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nhóm huy hiệu không được để trống")]
    public AchievementCategory Category { get; set; }

    [Required(ErrorMessage = "Icon không được để trống")]
    [MaxLength(50, ErrorMessage = "Icon không vượt quá 50 ký tự")]
    public string IconKey { get; set; } = string.Empty;

    [Range(0, int.MaxValue, ErrorMessage = "Thứ tự không được âm")]
    public int OrderNumber { get; set; }

    public bool IsUnlocked { get; set; }

    [Range(0, 100, ErrorMessage = "Tiến độ phải trong khoảng 0-100")]
    public int ProgressPercent { get; set; }
}

public class UpdateAchievementDto
{
    [Required(ErrorMessage = "Tên huy hiệu không được để trống")]
    [MaxLength(255, ErrorMessage = "Tên huy hiệu không vượt quá 255 ký tự")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mô tả không được để trống")]
    [MaxLength(500, ErrorMessage = "Mô tả không vượt quá 500 ký tự")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nhóm huy hiệu không được để trống")]
    public AchievementCategory Category { get; set; }

    [Required(ErrorMessage = "Icon không được để trống")]
    [MaxLength(50, ErrorMessage = "Icon không vượt quá 50 ký tự")]
    public string IconKey { get; set; } = string.Empty;

    [Range(0, int.MaxValue, ErrorMessage = "Thứ tự không được âm")]
    public int OrderNumber { get; set; }

    public bool IsUnlocked { get; set; }

    [Range(0, 100, ErrorMessage = "Tiến độ phải trong khoảng 0-100")]
    public int ProgressPercent { get; set; }
}

public class AchievementDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string IconKey { get; set; } = string.Empty;
    public int OrderNumber { get; set; }
    public bool IsUnlocked { get; set; }
    public int ProgressPercent { get; set; }
}
