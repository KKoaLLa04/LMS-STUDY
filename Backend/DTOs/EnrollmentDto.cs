namespace Backend.DTOs;

/// <summary>[Admin/Teacher] Ghi danh chỉ định một học sinh vào một khóa học (Backend/Controllers/EnrollmentsController.cs).</summary>
public class AdminEnrollDto
{
    public int UserId { get; set; }
    public int CourseId { get; set; }
}

public class EnrollmentDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public DateTime EnrolledAt { get; set; }

    // Chỉ được tính (khác 0 mặc định) khi lấy qua GetMyEnrollmentsAsync — GetByCourseAsync
    // (màn Admin/Teacher xem danh sách học sinh) không cần tiến độ nên bỏ qua cho nhẹ truy vấn.
    public int TotalLessons { get; set; }
    public int CompletedLessons { get; set; }
    public int ProgressPercent { get; set; }
}
