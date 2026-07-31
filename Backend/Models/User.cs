using System.ComponentModel.DataAnnotations;
using Backend.Common;

namespace Backend.Models;

public enum UserRole
{
    Admin,
    Teacher,
    Student
}

public enum UserStatus
{
    Active,
    Inactive,
    Banned
}

public enum UserGender
{
    Male,
    Female,
    Other
}

public class User : ISoftDelete
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    public UserRole Role { get; set; } = UserRole.Student;

    [Required]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    public string? AvatarUrl { get; set; }

    public UserStatus Status { get; set; } = UserStatus.Active;

    public UserGender? Gender { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? Address { get; set; }

    // Chỉ có ý nghĩa khi Role = Teacher — môn dạy/số năm kinh nghiệm/giới thiệu, admin nhập qua
    // form sửa user. Trước đây các trường này bị suy diễn giả ở Frontend vì chưa tồn tại ở DB.
    [MaxLength(100)]
    public string? Subject { get; set; }
    public int? ExperienceYears { get; set; }
    [MaxLength(1000)]
    public string? Bio { get; set; }

    // Chỉ có ý nghĩa khi Role = Student — khối/lớp đang học, dùng để lọc bảng xếp hạng theo khối.
    // Không khai báo FK tới KhoiHoc (cùng quy ước với Course.KhoiHocId): xóa KhoiHoc không bao
    // giờ bị chặn bởi các User đang tham chiếu tới nó.
    public int? KhoiHocId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    // Chuỗi ngày đăng nhập liên tiếp — cập nhật mỗi khi đăng nhập, dùng để tính điểm streak.
    public DateOnly? LastLoginDate { get; set; }
    public int LoginStreakCount { get; set; }

    // Chuỗi ngày có hoạt động học tập (xem bài học/làm quiz) liên tiếp — riêng biệt với
    // chuỗi đăng nhập vì đăng nhập không đồng nghĩa với việc học.
    public DateOnly? LastActivityDate { get; set; }
    public int HomeworkStreakCount { get; set; }
}
