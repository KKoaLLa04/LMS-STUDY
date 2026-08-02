using System.ComponentModel.DataAnnotations;

namespace Backend.Models;

// Một đáp án học sinh đã chọn cho một câu hỏi, tại một lần làm bài (QuizAttempt) cụ thể —
// mỗi option được chọn là 1 dòng (câu nhiều đáp án đúng sẽ có nhiều dòng cùng QuestionId).
// QuestionId/OptionId lưu dạng int thường (không ràng buộc FK) — cùng triết lý với
// QuizAttempt.QuizId (nullable): không muốn việc xóa/sửa câu hỏi sau này làm mất lịch sử đã làm.
public class QuizAttemptAnswerOption
{
    [Key]
    public int Id { get; set; }

    public int QuizAttemptId { get; set; }
    public QuizAttempt QuizAttempt { get; set; } = null!;

    public int QuestionId { get; set; }

    public int OptionId { get; set; }
}
