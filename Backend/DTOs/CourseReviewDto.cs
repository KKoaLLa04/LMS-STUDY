using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs;

public class CreateCourseReviewDto
{
    [Required(ErrorMessage = "Vui lòng chọn số sao đánh giá")]
    [Range(1, 5, ErrorMessage = "Đánh giá phải từ 1 đến 5 sao")]
    public int Rating { get; set; }

    [MaxLength(2000, ErrorMessage = "Nhận xét không vượt quá 2000 ký tự")]
    public string? Comment { get; set; }
}

public class CourseReviewDto
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public int UserId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsMine { get; set; }
}

public class CourseRatingBreakdownItemDto
{
    public int Stars { get; set; }
    public int Count { get; set; }
    public double Percent { get; set; }
}

public class CourseRatingSummaryDto
{
    public double AverageRating { get; set; }
    public int RatingCount { get; set; }
    public List<CourseRatingBreakdownItemDto> Breakdown { get; set; } = new();
}
