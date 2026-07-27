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

    [MaxLength(255, ErrorMessage = "Tên giáo viên không vượt quá 255 ký tự")]
    public string? Teacher { get; set; }

    [MaxLength(16, ErrorMessage = "Biểu tượng không vượt quá 16 ký tự")]
    public string? Emoji { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Giá không được âm")]
    public decimal Price { get; set; }

    public string Status { get; set; } = "Draft";

    public int? KhoiHocId { get; set; }
}

public class UpdateCourseDto
{
    [Required(ErrorMessage = "Tiêu đề không được để trống")]
    [MaxLength(255, ErrorMessage = "Tiêu đề không vượt quá 255 ký tự")]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [MaxLength(500, ErrorMessage = "Thumbnail không vượt quá 500 ký tự")]
    public string? Thumbnail { get; set; }

    [MaxLength(255, ErrorMessage = "Tên giáo viên không vượt quá 255 ký tự")]
    public string? Teacher { get; set; }

    [MaxLength(16, ErrorMessage = "Biểu tượng không vượt quá 16 ký tự")]
    public string? Emoji { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Giá không được âm")]
    public decimal Price { get; set; }

    public string Status { get; set; } = "Draft";

    public int? KhoiHocId { get; set; }
}

public class CourseListItemDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Thumbnail { get; set; }
    public string? Teacher { get; set; }
    public string? Emoji { get; set; }
    public decimal Price { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int? KhoiHocId { get; set; }
    public string? KhoiHocName { get; set; }
}

public class CourseDetailDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Thumbnail { get; set; }
    public string? Teacher { get; set; }
    public string? Emoji { get; set; }
    public decimal Price { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int? KhoiHocId { get; set; }
    public string? KhoiHocName { get; set; }
    public List<SectionDetailDto> Sections { get; set; } = new();
}
