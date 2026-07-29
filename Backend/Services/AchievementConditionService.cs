using Backend.Common;
using Backend.DTOs;
using Backend.Models;
using Backend.Services.Interfaces;

namespace Backend.Services;

public class AchievementConditionService : IAchievementConditionService
{
    // Metadata tĩnh cho từng loại điều kiện — dùng để render dropdown "Điều kiện đạt được"
    // ở Frontend (mã, tên hiển thị, đơn vị, gợi ý placeholder).
    private static readonly List<AchievementConditionTypeDto> Types = new()
    {
        new() { Code = nameof(AchievementConditionType.CompleteLessonCount), DisplayName = "Hoàn thành số bài học", Unit = "bài học", Placeholder = "Ví dụ: 10" },
        new() { Code = nameof(AchievementConditionType.CompleteCourseCount), DisplayName = "Hoàn thành số khóa học", Unit = "khóa học", Placeholder = "Ví dụ: 1" },
        new() { Code = nameof(AchievementConditionType.QuizPerfectScoreCount), DisplayName = "Số lần đạt điểm tuyệt đối (100%)", Unit = "lần", Placeholder = "Ví dụ: 3" },
        new() { Code = nameof(AchievementConditionType.QuizMinScorePercent), DisplayName = "Đạt điểm quiz tối thiểu", Unit = "%", Placeholder = "Ví dụ: 80" },
        new() { Code = nameof(AchievementConditionType.LoginStreakDays), DisplayName = "Chuỗi ngày đăng nhập liên tục", Unit = "ngày", Placeholder = "Ví dụ: 7" },
        new() { Code = nameof(AchievementConditionType.CommentCount), DisplayName = "Số bài viết/bình luận đã đăng", Unit = "bình luận", Placeholder = "Ví dụ: 5" },
        new() { Code = nameof(AchievementConditionType.ReceivedLikeCount), DisplayName = "Số lượt thích nhận được", Unit = "lượt thích", Placeholder = "Ví dụ: 20" },
        new() { Code = nameof(AchievementConditionType.FollowerCount), DisplayName = "Số người theo dõi", Unit = "người theo dõi", Placeholder = "Ví dụ: 10" },
        new() { Code = nameof(AchievementConditionType.RegisterBeforeHour), DisplayName = "Đăng ký trước giờ (trong ngày)", Unit = "giờ", Placeholder = "Ví dụ: 8 (trước 8h sáng)" }
    };

    public Task<ApiResponse<List<AchievementConditionTypeDto>>> GetTypesAsync() =>
        Task.FromResult(ApiResponse<List<AchievementConditionTypeDto>>.Ok(Types));
}
