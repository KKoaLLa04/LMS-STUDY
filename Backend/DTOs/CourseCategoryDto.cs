using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs;

public class CreateCourseCategoryDto
{
    [Required(ErrorMessage = "Tên danh mục không được để trống")]
    [MaxLength(255, ErrorMessage = "Tên danh mục không vượt quá 255 ký tự")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mã danh mục không được để trống")]
    [MaxLength(50, ErrorMessage = "Mã danh mục không vượt quá 50 ký tự")]
    public string Code { get; set; } = string.Empty;

    [Range(0, int.MaxValue, ErrorMessage = "Thứ tự không được âm")]
    public int OrderNumber { get; set; }
}

public class UpdateCourseCategoryDto
{
    [Required(ErrorMessage = "Tên danh mục không được để trống")]
    [MaxLength(255, ErrorMessage = "Tên danh mục không vượt quá 255 ký tự")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mã danh mục không được để trống")]
    [MaxLength(50, ErrorMessage = "Mã danh mục không vượt quá 50 ký tự")]
    public string Code { get; set; } = string.Empty;

    [Range(0, int.MaxValue, ErrorMessage = "Thứ tự không được âm")]
    public int OrderNumber { get; set; }
}

public class CourseCategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int OrderNumber { get; set; }
}
