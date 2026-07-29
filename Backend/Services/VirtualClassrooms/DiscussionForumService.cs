using Backend.Common;
using Backend.Data;
using Backend.DTOs;
using Backend.Models;
using Backend.Services.Interfaces;
using Backend.Services.VirtualClassrooms.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.VirtualClassrooms;

public class DiscussionForumService : IDiscussionForumService
{
    // Điểm tương tác cho mỗi bài viết/trả lời, và giới hạn số lượt được tính điểm mỗi ngày
    // để tránh học sinh spam bài viết rỗng nhằm farm điểm.
    private const int CreatePostPoints = 5;
    private const int ReplyPoints = 2;
    private const int MaxDailyInteractionAwards = 3;

    private readonly AppDbContext _context;
    private readonly IPointService _pointService;
    private readonly IAchievementEvaluationService _achievementEvaluationService;
    private readonly ILogger<DiscussionForumService> _logger;

    public DiscussionForumService(
        AppDbContext context,
        IPointService pointService,
        IAchievementEvaluationService achievementEvaluationService,
        ILogger<DiscussionForumService> logger)
    {
        _context = context;
        _pointService = pointService;
        _achievementEvaluationService = achievementEvaluationService;
        _logger = logger;
    }

    // Chỉ cộng điểm tương tác cho học sinh (không cộng khi Admin/Teacher là người tạo bài viết),
    // và giới hạn số lượt/ngày để chống spam farm điểm.
    private async Task AwardInteractionPointsAsync(int? currentUserId, int points, int postId, int courseId)
    {
        if (currentUserId is null) return;

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == currentUserId);
        if (user is null || user.Role != UserRole.Student) return;

        var todayUtc = DateTime.UtcNow.Date;
        var awardsToday = await _context.PointTransactions.CountAsync(t =>
            t.UserId == currentUserId &&
            t.SourceType == PointSourceType.DiscussionPost &&
            t.CreatedAt >= todayUtc);

        if (awardsToday >= MaxDailyInteractionAwards) return;

        await _pointService.AwardAsync(currentUserId.Value, points, PointSourceType.DiscussionPost, postId, courseId);
        await _pointService.RecordHomeworkStreakAsync(currentUserId.Value);
    }

    public async Task<ApiResponse<PagedResultDto<DiscussionPostListItemDto>>> GetPostsByCourseAsync(int courseId, int page, int pageSize, string? keyword, int? currentUserId)
    {
        try
        {
            var query = _context.DiscussionPosts
                .Include(p => p.Course)
                .Include(p => p.Replies)
                .Where(p => p.CourseId == courseId && p.ParentPostId == null);

            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(p => p.Title.Contains(keyword) || p.Content.Contains(keyword));

            query = query.OrderByDescending(p => p.CreatedAt);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new DiscussionPostListItemDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    AuthorName = p.AuthorName,
                    CourseId = p.CourseId,
                    CourseName = p.Course.Title,
                    ReplyCount = p.Replies.Count,
                    CreatedAt = p.CreatedAt,
                    LikeCount = _context.DiscussionPostLikes.Count(l => l.PostId == p.Id),
                    IsLikedByCurrentUser = currentUserId.HasValue &&
                        _context.DiscussionPostLikes.Any(l => l.PostId == p.Id && l.UserId == currentUserId)
                })
                .ToListAsync();

            var result = new PagedResultDto<DiscussionPostListItemDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };

            return ApiResponse<PagedResultDto<DiscussionPostListItemDto>>.Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách bài viết cho khóa học {CourseId}", courseId);
            return ApiResponse<PagedResultDto<DiscussionPostListItemDto>>.Error("Đã xảy ra lỗi khi lấy danh sách bài viết");
        }
    }

    public async Task<ApiResponse<DiscussionPostResponseDto>> GetPostByIdAsync(int id, int? currentUserId)
    {
        try
        {
            var post = await _context.DiscussionPosts
                .Include(p => p.Course)
                .Include(p => p.Replies).ThenInclude(r => r.Replies)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post is null)
                return ApiResponse<DiscussionPostResponseDto>.NotFound("Không tìm thấy bài viết");

            var allIds = CollectPostIds(post);
            var likeCounts = await _context.DiscussionPostLikes
                .Where(l => allIds.Contains(l.PostId))
                .GroupBy(l => l.PostId)
                .Select(g => new { PostId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.PostId, g => g.Count);

            var likedByMe = currentUserId.HasValue
                ? (await _context.DiscussionPostLikes
                    .Where(l => allIds.Contains(l.PostId) && l.UserId == currentUserId)
                    .Select(l => l.PostId)
                    .ToListAsync()).ToHashSet()
                : new HashSet<int>();

            return ApiResponse<DiscussionPostResponseDto>.Ok(MapToResponseDto(post, likeCounts, likedByMe));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy bài viết {Id}", id);
            return ApiResponse<DiscussionPostResponseDto>.Error("Đã xảy ra lỗi khi lấy bài viết");
        }
    }

    private static List<int> CollectPostIds(DiscussionPost p)
    {
        var ids = new List<int> { p.Id };
        foreach (var r in p.Replies) ids.AddRange(CollectPostIds(r));
        return ids;
    }

    public async Task<ApiResponse<DiscussionPostResponseDto>> CreatePostAsync(CreateDiscussionPostDto dto, int? currentUserId)
    {
        try
        {
            var courseExists = await _context.Courses.AnyAsync(c => c.Id == dto.CourseId);
            if (!courseExists)
                return ApiResponse<DiscussionPostResponseDto>.NotFound("Không tìm thấy khóa học");

            var post = new DiscussionPost
            {
                Title = dto.Title,
                Content = dto.Content,
                AuthorName = dto.AuthorName,
                UserId = currentUserId,
                CourseId = dto.CourseId,
                ParentPostId = null
            };

            _context.DiscussionPosts.Add(post);
            await _context.SaveChangesAsync();

            await _context.Entry(post).Reference(p => p.Course).LoadAsync();
            await AwardInteractionPointsAsync(currentUserId, CreatePostPoints, post.Id, post.CourseId);
            // Đếm cho điều kiện huy hiệu CommentCount — không phụ thuộc giới hạn điểm/ngày ở trên,
            // vì số lượng bài viết vẫn tăng kể cả khi đã hết lượt được tính điểm hôm nay.
            if (currentUserId.HasValue)
                await _achievementEvaluationService.EvaluateAsync(currentUserId.Value);

            return ApiResponse<DiscussionPostResponseDto>.Ok(MapToResponseDto(post), "Tạo bài viết thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tạo bài viết thảo luận");
            return ApiResponse<DiscussionPostResponseDto>.Error("Đã xảy ra lỗi khi tạo bài viết");
        }
    }

    public async Task<ApiResponse<DiscussionPostResponseDto>> UpdatePostAsync(int id, UpdateDiscussionPostDto dto)
    {
        try
        {
            var post = await _context.DiscussionPosts
                .Include(p => p.Course)
                .Include(p => p.Replies)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post is null)
                return ApiResponse<DiscussionPostResponseDto>.NotFound("Không tìm thấy bài viết");

            post.Title = dto.Title;
            post.Content = dto.Content;
            post.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return ApiResponse<DiscussionPostResponseDto>.Ok(MapToResponseDto(post), "Cập nhật bài viết thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi cập nhật bài viết {Id}", id);
            return ApiResponse<DiscussionPostResponseDto>.Error("Đã xảy ra lỗi khi cập nhật bài viết");
        }
    }

    public async Task<ApiResponse<object?>> DeletePostAsync(int id)
    {
        try
        {
            var exists = await _context.DiscussionPosts.AnyAsync(p => p.Id == id);
            if (!exists)
                return ApiResponse<object?>.NotFound("Không tìm thấy bài viết");

            await SoftDeletePostRecursiveAsync(id, DateTime.UtcNow);

            return ApiResponse<object?>.Ok(null, "Xóa bài viết thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xóa bài viết {Id}", id);
            return ApiResponse<object?>.Error("Đã xảy ra lỗi khi xóa bài viết");
        }
    }

    // Trước đây replies bị cascade xóa cứng theo FK ParentPostId; nay phải tự đệ quy
    // xóa mềm toàn bộ nhánh reply để không còn phản hồi "mồ côi" dưới bài viết đã xóa.
    private async Task SoftDeletePostRecursiveAsync(int postId, DateTime deletedAt)
    {
        var replyIds = await _context.DiscussionPosts
            .Where(p => p.ParentPostId == postId)
            .Select(p => p.Id)
            .ToListAsync();

        foreach (var replyId in replyIds)
        {
            await SoftDeletePostRecursiveAsync(replyId, deletedAt);
        }

        await _context.DiscussionPosts
            .Where(p => p.Id == postId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.IsDeleted, true)
                .SetProperty(x => x.DeletedAt, deletedAt));
    }

    public async Task<ApiResponse<DiscussionPostResponseDto>> ReplyToPostAsync(int parentPostId, CreateDiscussionPostDto dto, int? currentUserId)
    {
        try
        {
            var parentPost = await _context.DiscussionPosts.FirstOrDefaultAsync(p => p.Id == parentPostId);
            if (parentPost is null)
                return ApiResponse<DiscussionPostResponseDto>.NotFound("Không tìm thấy bài viết gốc");

            var reply = new DiscussionPost
            {
                Title = dto.Title,
                Content = dto.Content,
                AuthorName = dto.AuthorName,
                UserId = currentUserId,
                CourseId = parentPost.CourseId,
                ParentPostId = parentPostId
            };

            _context.DiscussionPosts.Add(reply);
            await _context.SaveChangesAsync();

            await _context.Entry(reply).Reference(p => p.Course).LoadAsync();
            await AwardInteractionPointsAsync(currentUserId, ReplyPoints, reply.Id, reply.CourseId);
            if (currentUserId.HasValue)
                await _achievementEvaluationService.EvaluateAsync(currentUserId.Value);

            return ApiResponse<DiscussionPostResponseDto>.Ok(MapToResponseDto(reply), "Trả lời bài viết thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi trả lời bài viết {ParentPostId}", parentPostId);
            return ApiResponse<DiscussionPostResponseDto>.Error("Đã xảy ra lỗi khi trả lời bài viết");
        }
    }

    public async Task<ApiResponse<object?>> LikePostAsync(int postId, int userId)
    {
        try
        {
            var post = await _context.DiscussionPosts.FirstOrDefaultAsync(p => p.Id == postId);
            if (post == null)
                return ApiResponse<object?>.NotFound("Không tìm thấy bài viết");

            var alreadyLiked = await _context.DiscussionPostLikes
                .AnyAsync(l => l.PostId == postId && l.UserId == userId);
            if (alreadyLiked)
                return ApiResponse<object?>.Ok(null, "Bạn đã thích bài viết này trước đó");

            _context.DiscussionPostLikes.Add(new DiscussionPostLike { PostId = postId, UserId = userId });
            await _context.SaveChangesAsync();

            // Tác giả bài viết có thể vừa đủ điều kiện huy hiệu ReceivedLikeCount.
            if (post.UserId.HasValue)
                await _achievementEvaluationService.EvaluateAsync(post.UserId.Value);

            return ApiResponse<object?>.Ok(null, "Đã thích bài viết");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi user {UserId} thích bài viết {PostId}", userId, postId);
            return ApiResponse<object?>.Error("Đã xảy ra lỗi khi thích bài viết");
        }
    }

    public async Task<ApiResponse<object?>> UnlikePostAsync(int postId, int userId)
    {
        try
        {
            var like = await _context.DiscussionPostLikes
                .FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);
            if (like == null)
                return ApiResponse<object?>.Ok(null, "Bạn chưa thích bài viết này");

            _context.DiscussionPostLikes.Remove(like);
            await _context.SaveChangesAsync();

            return ApiResponse<object?>.Ok(null, "Đã bỏ thích bài viết");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi user {UserId} bỏ thích bài viết {PostId}", userId, postId);
            return ApiResponse<object?>.Error("Đã xảy ra lỗi khi bỏ thích bài viết");
        }
    }

    private static DiscussionPostResponseDto MapToResponseDto(DiscussionPost p) =>
        MapToResponseDto(p, new Dictionary<int, int>(), new HashSet<int>());

    private static DiscussionPostResponseDto MapToResponseDto(DiscussionPost p, Dictionary<int, int> likeCounts, HashSet<int> likedByMe) => new()
    {
        Id = p.Id,
        Title = p.Title,
        Content = p.Content,
        AuthorName = p.AuthorName,
        CourseId = p.CourseId,
        CourseName = p.Course?.Title ?? string.Empty,
        ParentPostId = p.ParentPostId,
        Replies = p.Replies?.Select(r => MapToResponseDto(r, likeCounts, likedByMe)).ToList() ?? new(),
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt,
        LikeCount = likeCounts.GetValueOrDefault(p.Id, 0),
        IsLikedByCurrentUser = likedByMe.Contains(p.Id)
    };
}
