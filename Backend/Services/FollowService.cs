using Backend.Common;
using Backend.Data;
using Backend.DTOs;
using Backend.Models;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class FollowService : IFollowService
{
    private readonly AppDbContext _context;
    private readonly IAchievementEvaluationService _achievementEvaluationService;
    private readonly ILogger<FollowService> _logger;

    public FollowService(AppDbContext context, IAchievementEvaluationService achievementEvaluationService, ILogger<FollowService> logger)
    {
        _context = context;
        _achievementEvaluationService = achievementEvaluationService;
        _logger = logger;
    }

    public async Task<ApiResponse<object?>> FollowAsync(int followerId, int followingId)
    {
        try
        {
            if (followerId == followingId)
                return ApiResponse<object?>.BadRequest("Không thể tự theo dõi chính mình");

            var followingExists = await _context.Users.AnyAsync(u => u.Id == followingId);
            if (!followingExists)
                return ApiResponse<object?>.NotFound("Không tìm thấy người dùng cần theo dõi");

            var alreadyFollowing = await _context.Follows
                .AnyAsync(f => f.FollowerId == followerId && f.FollowingId == followingId);
            if (alreadyFollowing)
                return ApiResponse<object?>.Ok(null, "Bạn đã theo dõi người dùng này trước đó");

            _context.Follows.Add(new Follow { FollowerId = followerId, FollowingId = followingId });
            await _context.SaveChangesAsync();

            // Người được theo dõi có thể vừa đủ điều kiện huy hiệu FollowerCount.
            await _achievementEvaluationService.EvaluateAsync(followingId);

            return ApiResponse<object?>.Ok(null, "Theo dõi thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi user {FollowerId} theo dõi user {FollowingId}", followerId, followingId);
            return ApiResponse<object?>.Error("Đã xảy ra lỗi khi theo dõi người dùng");
        }
    }

    public async Task<ApiResponse<object?>> UnfollowAsync(int followerId, int followingId)
    {
        try
        {
            var follow = await _context.Follows
                .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FollowingId == followingId);
            if (follow == null)
                return ApiResponse<object?>.Ok(null, "Bạn chưa theo dõi người dùng này");

            _context.Follows.Remove(follow);
            await _context.SaveChangesAsync();

            return ApiResponse<object?>.Ok(null, "Bỏ theo dõi thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi user {FollowerId} bỏ theo dõi user {FollowingId}", followerId, followingId);
            return ApiResponse<object?>.Error("Đã xảy ra lỗi khi bỏ theo dõi người dùng");
        }
    }

    public async Task<ApiResponse<FollowStatusDto>> GetStatusAsync(int currentUserId, int targetUserId)
    {
        try
        {
            var isFollowing = await _context.Follows
                .AnyAsync(f => f.FollowerId == currentUserId && f.FollowingId == targetUserId);
            var followersCount = await _context.Follows.CountAsync(f => f.FollowingId == targetUserId);
            var followingCount = await _context.Follows.CountAsync(f => f.FollowerId == targetUserId);

            return ApiResponse<FollowStatusDto>.Ok(new FollowStatusDto
            {
                IsFollowing = isFollowing,
                FollowersCount = followersCount,
                FollowingCount = followingCount
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy trạng thái theo dõi cho user {TargetUserId}", targetUserId);
            return ApiResponse<FollowStatusDto>.Error("Đã xảy ra lỗi khi lấy trạng thái theo dõi");
        }
    }
}
