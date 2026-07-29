namespace Backend.Services.Interfaces;

public interface IAchievementEvaluationService
{
    // Kiểm tra toàn bộ AchievementCondition của các huy hiệu user chưa mở khóa, tự động
    // trao huy hiệu (UserAchievement + điểm thưởng) nếu đủ điều kiện. An toàn để gọi lại
    // nhiều lần (bỏ qua huy hiệu đã mở khóa) — gọi ngay sau bất kỳ hành động nào có thể
    // ảnh hưởng tới điều kiện huy hiệu (hoàn thành bài học, nộp quiz, đăng bài, đăng nhập...).
    Task EvaluateAsync(int userId);
}
