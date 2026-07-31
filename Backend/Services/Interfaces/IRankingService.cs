using Backend.Common;
using Backend.DTOs;

namespace Backend.Services.Interfaces;

public interface IRankingService
{
    // courseId == null => xếp hạng toàn hệ thống; có giá trị => xếp hạng trong phạm vi lớp/khóa học đó.
    // khoiHocId có giá trị => chỉ xếp hạng học sinh thuộc khối đó (User.KhoiHocId).
    Task<ApiResponse<LeaderboardResponseDto>> GetLeaderboardAsync(RankPeriod period, int? courseId, int? khoiHocId, int currentUserId, int page, int pageSize);
}
