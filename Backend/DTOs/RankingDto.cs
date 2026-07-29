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
}

public class LeaderboardResponseDto
{
    public PagedResultDto<RankingEntryDto> Items { get; set; } = new();

    // Vị trí/điểm của người gọi API — luôn kèm theo dù không nằm trong trang hiện tại,
    // để hiển thị "hạng của bạn" ngay cả khi đang ở ngoài top hiển thị.
    public RankingEntryDto? Me { get; set; }
}
