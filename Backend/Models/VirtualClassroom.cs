using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

public enum MeetingPlatform { Zoom, GoogleMeet, MSTeams }
public enum ClassroomStatus { Scheduled, Active, Ended, Cancelled }

public class VirtualClassroom
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public MeetingPlatform Platform { get; set; } = MeetingPlatform.Zoom;

    [MaxLength(500)]
    public string? MeetingUrl { get; set; }

    [MaxLength(100)]
    public string? MeetingId { get; set; }

    [MaxLength(100)]
    public string? MeetingPassword { get; set; }

    public DateTime ScheduledAt { get; set; }

    public int DurationMinutes { get; set; } = 60;

    public ClassroomStatus Status { get; set; } = ClassroomStatus.Scheduled;

    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
