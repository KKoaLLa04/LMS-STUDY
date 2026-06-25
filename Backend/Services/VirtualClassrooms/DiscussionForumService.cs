using Backend.Common;
using Backend.Data;
using Backend.DTOs;
using Backend.Models;
using Backend.Services.VirtualClassrooms.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.VirtualClassrooms;

public class DiscussionForumService : IDiscussionForumService
{
    private readonly AppDbContext _context;
    private readonly ILogger<DiscussionForumService> _logger;

    public DiscussionForumService(AppDbContext context, ILogger<DiscussionForumService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<PagedResultDto<DiscussionPostListItemDto>>> GetPostsByCourseAsync(int courseId, int page, int pageSize, string? keyword)
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
                    CreatedAt = p.CreatedAt
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

    public async Task<ApiResponse<DiscussionPostResponseDto>> GetPostByIdAsync(int id)
    {
        try
        {
            var post = await _context.DiscussionPosts
                .Include(p => p.Course)
                .Include(p => p.Replies).ThenInclude(r => r.Replies)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post is null)
                return ApiResponse<DiscussionPostResponseDto>.NotFound("Không tìm thấy bài viết");

            return ApiResponse<DiscussionPostResponseDto>.Ok(MapToResponseDto(post));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy bài viết {Id}", id);
            return ApiResponse<DiscussionPostResponseDto>.Error("Đã xảy ra lỗi khi lấy bài viết");
        }
    }

    public async Task<ApiResponse<DiscussionPostResponseDto>> CreatePostAsync(CreateDiscussionPostDto dto)
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
                CourseId = dto.CourseId,
                ParentPostId = null
            };

            _context.DiscussionPosts.Add(post);
            await _context.SaveChangesAsync();

            await _context.Entry(post).Reference(p => p.Course).LoadAsync();

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
            var post = await _context.DiscussionPosts.FindAsync(id);
            if (post is null)
                return ApiResponse<object?>.NotFound("Không tìm thấy bài viết");

            _context.DiscussionPosts.Remove(post);
            await _context.SaveChangesAsync();

            return ApiResponse<object?>.Ok(null, "Xóa bài viết thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xóa bài viết {Id}", id);
            return ApiResponse<object?>.Error("Đã xảy ra lỗi khi xóa bài viết");
        }
    }

    public async Task<ApiResponse<DiscussionPostResponseDto>> ReplyToPostAsync(int parentPostId, CreateDiscussionPostDto dto)
    {
        try
        {
            var parentPost = await _context.DiscussionPosts.FindAsync(parentPostId);
            if (parentPost is null)
                return ApiResponse<DiscussionPostResponseDto>.NotFound("Không tìm thấy bài viết gốc");

            var reply = new DiscussionPost
            {
                Title = dto.Title,
                Content = dto.Content,
                AuthorName = dto.AuthorName,
                CourseId = parentPost.CourseId,
                ParentPostId = parentPostId
            };

            _context.DiscussionPosts.Add(reply);
            await _context.SaveChangesAsync();

            await _context.Entry(reply).Reference(p => p.Course).LoadAsync();

            return ApiResponse<DiscussionPostResponseDto>.Ok(MapToResponseDto(reply), "Trả lời bài viết thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi trả lời bài viết {ParentPostId}", parentPostId);
            return ApiResponse<DiscussionPostResponseDto>.Error("Đã xảy ra lỗi khi trả lời bài viết");
        }
    }

    private static DiscussionPostResponseDto MapToResponseDto(DiscussionPost p) => new()
    {
        Id = p.Id,
        Title = p.Title,
        Content = p.Content,
        AuthorName = p.AuthorName,
        CourseId = p.CourseId,
        CourseName = p.Course?.Title ?? string.Empty,
        ParentPostId = p.ParentPostId,
        Replies = p.Replies?.Select(r => MapToResponseDto(r)).ToList() ?? new(),
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt
    };
}
