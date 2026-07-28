using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs;

public class UpdateLessonProgressDto
{
    [Range(0, int.MaxValue, ErrorMessage = "Số giây đã xem không được âm")]
    public int WatchedSeconds { get; set; }
}

public class LessonProgressDto
{
    public int LessonId { get; set; }
    public string LessonTitle { get; set; } = string.Empty;
    public int WatchedSeconds { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
}
