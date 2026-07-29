using Backend.Common;
using Backend.DTOs;

namespace Backend.Services.Interfaces;

public interface IFollowService
{
    Task<ApiResponse<object?>> FollowAsync(int followerId, int followingId);
    Task<ApiResponse<object?>> UnfollowAsync(int followerId, int followingId);
    Task<ApiResponse<FollowStatusDto>> GetStatusAsync(int currentUserId, int targetUserId);
}
