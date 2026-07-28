using Backend.Common;
using Backend.DTOs;

namespace Backend.Services.Interfaces;

public interface IRankingService
{
    // courseId == null => xếp hạng toàn hệ thống; có giá trị => xếp hạng trong phạm vi lớp/khóa học đó.
    Task<ApiResponse<LeaderboardResponseDto>> GetLeaderboardAsync(RankPeriod period, int? courseId, int currentUserId, int page, int pageSize);
}
