namespace Backend.DTOs;

public class EnrollmentDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public DateTime EnrolledAt { get; set; }
}
