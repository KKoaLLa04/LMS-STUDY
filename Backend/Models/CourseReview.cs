using System.ComponentModel.DataAnnotations;
using Backend.Common;

namespace Backend.Models;

// Mỗi học sinh chỉ được viết 1 đánh giá cho 1 khóa học (unique index CourseId+UserId ở
// AppDbContext) — viết lại sẽ cập nhật đánh giá cũ thay vì tạo bản ghi mới.
public class CourseReview : ISoftDelete
{
    [Key]
    public int Id { get; set; }

    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    [Range(1, 5)]
    public int Rating { get; set; }

    [MaxLength(2000)]
    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}
