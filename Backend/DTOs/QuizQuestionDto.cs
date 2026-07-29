using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs;

// ---- Dành cho học sinh (KHÔNG bao giờ được chứa IsCorrect) ----

public class QuizOptionDto
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
}

public class QuizQuestionDto
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public int OrderNumber { get; set; }
    public bool AllowMultipleAnswers { get; set; }
    public List<QuizOptionDto> Options { get; set; } = new();
}

// ---- Dành cho Admin soạn câu hỏi (có IsCorrect) ----

public class QuizOptionAdminDto
{
    // Id null nghĩa là option mới; có giá trị nếu là option đã tồn tại (không thực sự dùng để
    // giữ nguyên khi lưu — xem ghi chú replace-all trong QuizQuestionService — nhưng vẫn trả về
    // để phía Admin UI có thể track theo id khi hiển thị).
    public int? Id { get; set; }

    [Required(ErrorMessage = "Nội dung đáp án không được để trống")]
    public string Text { get; set; } = string.Empty;

    public bool IsCorrect { get; set; }

    public int OrderNumber { get; set; }
}

public class QuizQuestionAdminDto
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Nội dung câu hỏi không được để trống")]
    public string Text { get; set; } = string.Empty;

    public int OrderNumber { get; set; }

    public bool AllowMultipleAnswers { get; set; }

    [MinLength(2, ErrorMessage = "Câu hỏi cần ít nhất 2 đáp án")]
    public List<QuizOptionAdminDto> Options { get; set; } = new();
}

// Toàn bộ câu hỏi của một bài học Quiz được thay thế trọn vẹn mỗi lần lưu (replace-all) —
// đơn giản hơn nhiều so với diff từng câu hỏi/đáp án, và chấp nhận được vì chưa có phân tích
// nào gắn với QuestionId/OptionId cụ thể (QuizAttempt chỉ lưu ScorePercent tổng).
public class ReplaceQuizQuestionsDto
{
    public List<QuizQuestionAdminDto> Questions { get; set; } = new();
}
