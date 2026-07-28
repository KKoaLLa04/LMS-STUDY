using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Backend.Common;

namespace Backend.Models;

public enum CourseStatus
{
    Draft,
    Published,
    Upcoming
}

public class Course : ISoftDelete
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [MaxLength(500)]
    public string? Thumbnail { get; set; }

    [MaxLength(255)]
    public string? Teacher { get; set; }

    [MaxLength(16)]
    public string? Emoji { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    public CourseStatus Status { get; set; } = CourseStatus.Draft;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Lưu Id của KhoiHoc như một giá trị thường, không cấu hình quan hệ khóa ngoại —
    // để việc xóa KhoiHoc không bao giờ bị chặn bởi các Course đang tham chiếu tới nó.
    public int? KhoiHocId { get; set; }

    // Cùng cách xử lý như KhoiHocId: giá trị thường, không FK, để xóa CourseCategory
    // không bao giờ bị chặn bởi các Course đang tham chiếu tới nó.
    public int? CategoryId { get; set; }

    public bool IsFeatured { get; set; }

    [MaxLength(500)]
    public string? PreviewVideoUrl { get; set; }

    public ICollection<Section> Sections { get; set; } = new List<Section>();
}
