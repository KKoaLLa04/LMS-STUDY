using System.ComponentModel.DataAnnotations;
using Backend.Common;

namespace Backend.Models;

// Ghi nhận học sinh nào thuộc khóa học (lớp) nào — cần thiết để tính bảng xếp hạng "theo lớp"
// và để biết phạm vi CourseId hợp lệ khi liệt kê học sinh của một khóa học.
public class Enrollment : ISoftDelete
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;

    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}
