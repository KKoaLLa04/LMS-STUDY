using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs;

public class CreateVirtualClassroomDto
{
    [Required(ErrorMessage = "Tiêu đề không được để trống")]
    [MaxLength(255, ErrorMessage = "Tiêu đề không vượt quá 255 ký tự")]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required(ErrorMessage = "Nền tảng không được để trống")]
    public string Platform { get; set; } = "Zoom";

    [MaxLength(500, ErrorMessage = "URL không vượt quá 500 ký tự")]
    public string? MeetingUrl { get; set; }

    [MaxLength(100)]
    public string? MeetingId { get; set; }

    [MaxLength(100)]
    public string? MeetingPassword { get; set; }

    [Required(ErrorMessage = "Thời gian bắt đầu không được để trống")]
    public DateTime ScheduledAt { get; set; }

    [Range(15, 480, ErrorMessage = "Thời lượng từ 15 đến 480 phút")]
    public int DurationMinutes { get; set; } = 60;

    [Required]
    public int CourseId { get; set; }
}

public class UpdateVirtualClassroomDto
{
    [Required(ErrorMessage = "Tiêu đề không được để trống")]
    [MaxLength(255, ErrorMessage = "Tiêu đề không vượt quá 255 ký tự")]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    public string Platform { get; set; } = "Zoom";

    [MaxLength(500)]
    public string? MeetingUrl { get; set; }

    [MaxLength(100)]
    public string? MeetingId { get; set; }

    [MaxLength(100)]
    public string? MeetingPassword { get; set; }

    [Required]
    public DateTime ScheduledAt { get; set; }

    [Range(15, 480)]
    public int DurationMinutes { get; set; } = 60;

    public string Status { get; set; } = "Scheduled";
}

public class VirtualClassroomResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Platform { get; set; } = string.Empty;
    public string? MeetingUrl { get; set; }
    public string? MeetingId { get; set; }
    public string? MeetingPassword { get; set; }
    public DateTime ScheduledAt { get; set; }
    public int DurationMinutes { get; set; }
    public string Status { get; set; } = string.Empty;
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
