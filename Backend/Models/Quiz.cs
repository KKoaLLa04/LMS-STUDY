using System.ComponentModel.DataAnnotations;
using Backend.Common;

namespace Backend.Models;

// Quiz dùng chung — không bắt buộc thuộc bài học/khóa học nào. Có thể được nhiều Lesson
// (Lesson.QuizId) tham chiếu tới cùng một Quiz, và học viên cũng có thể làm trực tiếp qua
// trang "Quiz chung" mà không cần qua khóa học. Câu hỏi (QuizQuestion) thuộc về Quiz, không
// còn thuộc trực tiếp về Lesson.
public class Quiz : ISoftDelete
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    public ICollection<QuizQuestion> Questions { get; set; } = new List<QuizQuestion>();
}
