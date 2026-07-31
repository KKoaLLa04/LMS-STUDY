using Backend.Common;
using Backend.Data;
using Backend.DTOs;
using Backend.Models;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class CourseReviewService : ICourseReviewService
{
    private readonly AppDbContext _context;
    private readonly ILogger<CourseReviewService> _logger;

    public CourseReviewService(AppDbContext context, ILogger<CourseReviewService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<CourseRatingSummaryDto>> GetSummaryAsync(int courseId)
    {
        try
        {
            var ratings = await _context.CourseReviews
                .Where(r => r.CourseId == courseId)
                .Select(r => r.Rating)
                .ToListAsync();

            var ratingCount = ratings.Count;
            var breakdown = Enumerable.Range(1, 5)
                .Select(stars => new CourseRatingBreakdownItemDto
                {
                    Stars = stars,
                    Count = ratings.Count(r => r == stars),
                    Percent = ratingCount == 0 ? 0 : Math.Round(ratings.Count(r => r == stars) * 100.0 / ratingCount, 1)
                })
                .OrderByDescending(b => b.Stars)
                .ToList();

            var summary = new CourseRatingSummaryDto
            {
                AverageRating = ratingCount == 0 ? 0 : Math.Round(ratings.Average(), 1),
                RatingCount = ratingCount,
                Breakdown = breakdown
            };

            return ApiResponse<CourseRatingSummaryDto>.Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy tổng hợp đánh giá của khóa học {CourseId}", courseId);
            return ApiResponse<CourseRatingSummaryDto>.Error("Đã xảy ra lỗi khi lấy tổng hợp đánh giá");
        }
    }

    public async Task<ApiResponse<PagedResultDto<CourseReviewDto>>> GetByCourseAsync(int courseId, int page, int pageSize, int? currentUserId)
    {
        try
        {
            var query = _context.CourseReviews
                .Include(r => r.User)
                .Where(r => r.CourseId == courseId)
                .OrderByDescending(r => r.CreatedAt);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new CourseReviewDto
                {
                    Id = r.Id,
                    CourseId = r.CourseId,
                    UserId = r.UserId,
                    StudentName = r.User.FullName,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt,
                    IsMine = currentUserId.HasValue && r.UserId == currentUserId
                })
                .ToListAsync();

            var result = new PagedResultDto<CourseReviewDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalCount == 0 ? 0 : (int)Math.Ceiling((double)totalCount / pageSize)
            };

            return ApiResponse<PagedResultDto<CourseReviewDto>>.Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách đánh giá của khóa học {CourseId}", courseId);
            return ApiResponse<PagedResultDto<CourseReviewDto>>.Error("Đã xảy ra lỗi khi lấy danh sách đánh giá");
        }
    }

    // Chỉ học sinh đã ghi danh khóa học mới được đánh giá (tương tự "verified purchase") —
    // tránh đánh giá ảo từ tài khoản chưa từng học. Gọi lại lần 2 sẽ cập nhật đánh giá cũ
    // thay vì tạo bản ghi mới, nhờ unique index (CourseId, UserId).
    public async Task<ApiResponse<CourseReviewDto>> CreateOrUpdateAsync(int courseId, int userId, CreateCourseReviewDto dto)
    {
        try
        {
            var isEnrolled = await _context.Enrollments
                .AnyAsync(e => e.CourseId == courseId && e.UserId == userId);
            if (!isEnrolled)
                return ApiResponse<CourseReviewDto>.BadRequest("Bạn cần ghi danh khóa học này trước khi đánh giá");

            var review = await _context.CourseReviews
                .FirstOrDefaultAsync(r => r.CourseId == courseId && r.UserId == userId);

            if (review is null)
            {
                review = new CourseReview
                {
                    CourseId = courseId,
                    UserId = userId,
                    Rating = dto.Rating,
                    Comment = dto.Comment
                };
                _context.CourseReviews.Add(review);
            }
            else
            {
                review.Rating = dto.Rating;
                review.Comment = dto.Comment;
                review.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            await _context.Entry(review).Reference(r => r.User).LoadAsync();

            return ApiResponse<CourseReviewDto>.Ok(new CourseReviewDto
            {
                Id = review.Id,
                CourseId = review.CourseId,
                UserId = review.UserId,
                StudentName = review.User.FullName,
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedAt = review.CreatedAt,
                UpdatedAt = review.UpdatedAt,
                IsMine = true
            }, "Gửi đánh giá thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi user {UserId} đánh giá khóa học {CourseId}", userId, courseId);
            return ApiResponse<CourseReviewDto>.Error("Đã xảy ra lỗi khi gửi đánh giá");
        }
    }

    public async Task<ApiResponse<object?>> DeleteAsync(int id, int currentUserId, bool isAdmin)
    {
        try
        {
            var review = await _context.CourseReviews.FirstOrDefaultAsync(r => r.Id == id);
            if (review is null)
                return ApiResponse<object?>.NotFound("Không tìm thấy đánh giá");

            if (!isAdmin && review.UserId != currentUserId)
                return ApiResponse<object?>.Unauthorized("Bạn không có quyền xóa đánh giá này");

            review.IsDeleted = true;
            review.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return ApiResponse<object?>.Ok(null, "Xóa đánh giá thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xóa đánh giá {Id}", id);
            return ApiResponse<object?>.Error("Đã xảy ra lỗi khi xóa đánh giá");
        }
    }
}
