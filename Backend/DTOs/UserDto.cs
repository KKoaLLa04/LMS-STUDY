using System.ComponentModel.DataAnnotations;
using Backend.Models;

namespace Backend.DTOs;

public class CreateUserDto
{
    [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
    [MaxLength(100, ErrorMessage = "Tên đăng nhập không vượt quá 100 ký tự")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mật khẩu không được để trống")]
    [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email không được để trống")]
    [MaxLength(255, ErrorMessage = "Email không vượt quá 255 ký tự")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Họ tên không được để trống")]
    [MaxLength(255, ErrorMessage = "Họ tên không vượt quá 255 ký tự")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Số điện thoại không được để trống")]
    [MaxLength(20, ErrorMessage = "Số điện thoại không vượt quá 20 ký tự")]
    public string Phone { get; set; } = string.Empty;

    public string? AvatarUrl { get; set; }

    public UserStatus Status { get; set; } = UserStatus.Active;

    public UserGender? Gender { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? Address { get; set; }

    [Required(ErrorMessage = "Vai trò không được để trống")]
    public UserRole Role { get; set; }
}

public class UpdateUserDto
{
    [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
    [MaxLength(100, ErrorMessage = "Tên đăng nhập không vượt quá 100 ký tự")]
    public string Username { get; set; } = string.Empty;

    [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
    public string? Password { get; set; }

    [Required(ErrorMessage = "Email không được để trống")]
    [MaxLength(255, ErrorMessage = "Email không vượt quá 255 ký tự")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Họ tên không được để trống")]
    [MaxLength(255, ErrorMessage = "Họ tên không vượt quá 255 ký tự")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Số điện thoại không được để trống")]
    [MaxLength(20, ErrorMessage = "Số điện thoại không vượt quá 20 ký tự")]
    public string Phone { get; set; } = string.Empty;

    public string? AvatarUrl { get; set; }

    public UserStatus Status { get; set; } = UserStatus.Active;

    public UserGender? Gender { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? Address { get; set; }

    [Required(ErrorMessage = "Vai trò không được để trống")]
    public UserRole Role { get; set; }
}

public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Gender { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? Address { get; set; }
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

// Thông tin công khai tối thiểu — dùng cho các trang hiển thị người dùng khác (vd. hồ sơ giáo viên),
// không lộ email/số điện thoại/địa chỉ như UserDto đầy đủ (chỉ Admin mới xem được UserDto).
public class PublicUserDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string Role { get; set; } = string.Empty;
    public string? Gender { get; set; }
}
