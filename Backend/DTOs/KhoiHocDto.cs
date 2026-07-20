using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs;

public class CreateKhoiHocDto
{
    [Required(ErrorMessage = "Tên khối học không được để trống")]
    [MaxLength(255, ErrorMessage = "Tên khối học không vượt quá 255 ký tự")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mã khối học không được để trống")]
    [MaxLength(50, ErrorMessage = "Mã khối học không vượt quá 50 ký tự")]
    public string Code { get; set; } = string.Empty;

    [Range(0, int.MaxValue, ErrorMessage = "Thứ tự không được âm")]
    public int OrderNumber { get; set; }
}

public class UpdateKhoiHocDto
{
    [Required(ErrorMessage = "Tên khối học không được để trống")]
    [MaxLength(255, ErrorMessage = "Tên khối học không vượt quá 255 ký tự")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mã khối học không được để trống")]
    [MaxLength(50, ErrorMessage = "Mã khối học không vượt quá 50 ký tự")]
    public string Code { get; set; } = string.Empty;

    [Range(0, int.MaxValue, ErrorMessage = "Thứ tự không được âm")]
    public int OrderNumber { get; set; }
}

public class KhoiHocDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int OrderNumber { get; set; }
}
