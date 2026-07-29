using System.ComponentModel.DataAnnotations;
using Backend.Common;

namespace Backend.Models;

// Một câu hỏi trắc nghiệm thuộc về một Quiz dùng chung (Quiz có thể được nhiều Lesson tham
// chiếu qua Lesson.QuizId). AllowMultipleAnswers quyết định UI hiển thị radio (1 đáp án) hay
// checkbox (nhiều đáp án) — việc chấm điểm (QuizService) so khớp chính xác tập đáp án đã chọn
// với tập đáp án đúng, áp dụng như nhau cho cả hai dạng, không có điểm một phần.
public class QuizQuestion : ISoftDelete
{
    [Key]
    public int Id { get; set; }

    public int QuizId { get; set; }
    public Quiz Quiz { get; set; } = null!;

    [Required]
    public string Text { get; set; } = string.Empty;

    public int OrderNumber { get; set; }

    public bool AllowMultipleAnswers { get; set; }

    public ICollection<QuizOption> Options { get; set; } = new List<QuizOption>();

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}
