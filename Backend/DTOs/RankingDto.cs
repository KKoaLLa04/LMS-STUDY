using Backend.Common;

namespace Backend.DTOs;

public enum RankPeriod
{
    Week,
    Month,
    All
}

public class RankingEntryDto
{
    public int Rank { get; set; }
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public int TotalPoints { get; set; }
    public bool IsMe { get; set; }
    public bool IsFollowing { get; set; }
    public int? KhoiHocId { get; set; }
    public string? GradeName { get; set; }

    // Hạng của cùng người này ở "kỳ trước" (tuần/tháng liền trước, hoặc 7 ngày trước với period=All),
    // tính lại trực tiếp từ PointTransactions — không cần bảng snapshot lịch sử riêng.
    // Null nếu người này chưa có giao dịch điểm nào ở kỳ trước (chưa có dữ liệu để so sánh).
    public int? PreviousRank { get; set; }
}

public class LeaderboardResponseDto
{
    public PagedResultDto<RankingEntryDto> Items { get; set; } = new();

    // Vị trí/điểm của người gọi API — luôn kèm theo dù không nằm trong trang hiện tại,
    // để hiển thị "hạng của bạn" ngay cả khi đang ở ngoài top hiển thị.
    public RankingEntryDto? Me { get; set; }
}
