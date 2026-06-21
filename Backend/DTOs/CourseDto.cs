using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs;

public class CreateCourseDto
{
    [Required(ErrorMessage = "Tiêu đề không được để trống")]
    [MaxLength(255, ErrorMessage = "Tiêu đề không vượt quá 255 ký tự")]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [MaxLength(500, ErrorMessage = "Thumbnail không vượt quá 500 ký tự")]
    public string? Thumbnail { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Giá không được âm")]
    public decimal Price { get; set; }

    public string Status { get; set; } = "Draft";
}

public class UpdateCourseDto
{
    [Required(ErrorMessage = "Tiêu đề không được để trống")]
    [MaxLength(255, ErrorMessage = "Tiêu đề không vượt quá 255 ký tự")]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [MaxLength(500, ErrorMessage = "Thumbnail không vượt quá 500 ký tự")]
    public string? Thumbnail { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Giá không được âm")]
    public decimal Price { get; set; }

    public string Status { get; set; } = "Draft";
}

public class CourseListItemDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Thumbnail { get; set; }
    public decimal Price { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CourseDetailDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Thumbnail { get; set; }
    public decimal Price { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<SectionDetailDto> Sections { get; set; } = new();
}
