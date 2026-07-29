using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs;

public class CreateAchievementGroupDto
{
    [Required(ErrorMessage = "Tên nhóm huy hiệu không được để trống")]
    [MaxLength(255, ErrorMessage = "Tên nhóm huy hiệu không vượt quá 255 ký tự")]
    public string Name { get; set; } = string.Empty;
}

public class AchievementGroupDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int OrderNumber { get; set; }
}
