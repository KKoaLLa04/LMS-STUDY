using Backend.Models;

namespace Backend.Services.Interfaces;

public interface IPointService
{
    // Ghi một giao dịch điểm vào sổ cái. points <= 0 sẽ bị bỏ qua (không có gì để cộng).
    Task AwardAsync(int userId, int points, PointSourceType sourceType, int? sourceRefId = null, int? courseId = null);

    // Cập nhật chuỗi ngày đăng nhập liên tiếp của user và cộng điểm thưởng streak nếu hôm nay
    // là ngày mới (idempotent trong cùng một ngày — gọi nhiều lần/ngày chỉ tính 1 lần).
    Task RecordLoginStreakAsync(int userId);

    // Cập nhật chuỗi ngày có hoạt động học tập (xem bài học/làm quiz) và cộng điểm thưởng.
    // Gọi từ LessonProgressService/QuizService mỗi khi có hoạt động học tập trong ngày.
    Task RecordHomeworkStreakAsync(int userId);
}
