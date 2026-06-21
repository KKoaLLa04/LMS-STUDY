using Backend.Common;
using Backend.Data;
using Backend.DTOs;
using Backend.Models;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class CourseService : ICourseService
{
    private readonly AppDbContext _context;
    private readonly ILogger<CourseService> _logger;

    public CourseService(AppDbContext context, ILogger<CourseService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<PagedResultDto<CourseListItemDto>>> GetCoursesAsync(
        int page, int pageSize, string? keyword)
    {
        try
        {
            var query = _context.Courses.AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(c => c.Title.Contains(keyword));

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CourseListItemDto
                {
                    Id = c.Id,
                    Title = c.Title,
                    Thumbnail = c.Thumbnail,
                    Price = c.Price,
                    Status = c.Status.ToString(),
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();

            var result = new PagedResultDto<CourseListItemDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalCount == 0 ? 0 : (int)Math.Ceiling((double)totalCount / pageSize)
            };

            return ApiResponse<PagedResultDto<CourseListItemDto>>.Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách khóa học");
            return ApiResponse<PagedResultDto<CourseListItemDto>>.Error("Đã xảy ra lỗi khi lấy danh sách khóa học");
        }
    }

    public async Task<ApiResponse<CourseDetailDto>> GetCourseByIdAsync(int id)
    {
        try
        {
            var course = await _context.Courses
                .Include(c => c.Sections.OrderBy(s => s.Position))
                    .ThenInclude(s => s.Lessons.OrderBy(l => l.Position))
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
                return ApiResponse<CourseDetailDto>.NotFound("Khóa học không tồn tại");

            var dto = MapToCourseDetailDto(course);
            return ApiResponse<CourseDetailDto>.Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy chi tiết khóa học {Id}", id);
            return ApiResponse<CourseDetailDto>.Error("Đã xảy ra lỗi khi lấy thông tin khóa học");
        }
    }

    public async Task<ApiResponse<CourseListItemDto>> CreateCourseAsync(CreateCourseDto dto)
    {
        try
        {
            if (!Enum.TryParse<CourseStatus>(dto.Status, ignoreCase: true, out var status))
                return ApiResponse<CourseListItemDto>.BadRequest(
                    "Trạng thái không hợp lệ. Chỉ chấp nhận: Draft hoặc Published");

            var course = new Course
            {
                Title = dto.Title.Trim(),
                Description = dto.Description?.Trim(),
                Thumbnail = dto.Thumbnail?.Trim(),
                Price = dto.Price,
                Status = status
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            return ApiResponse<CourseListItemDto>.Ok(MapToCourseListItemDto(course), "Tạo khóa học thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tạo khóa học");
            return ApiResponse<CourseListItemDto>.Error("Đã xảy ra lỗi khi tạo khóa học");
        }
    }

    public async Task<ApiResponse<CourseListItemDto>> UpdateCourseAsync(int id, UpdateCourseDto dto)
    {
        try
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
                return ApiResponse<CourseListItemDto>.NotFound("Khóa học không tồn tại");

            if (!Enum.TryParse<CourseStatus>(dto.Status, ignoreCase: true, out var status))
                return ApiResponse<CourseListItemDto>.BadRequest(
                    "Trạng thái không hợp lệ. Chỉ chấp nhận: Draft hoặc Published");

            course.Title = dto.Title.Trim();
            course.Description = dto.Description?.Trim();
            course.Thumbnail = dto.Thumbnail?.Trim();
            course.Price = dto.Price;
            course.Status = status;

            await _context.SaveChangesAsync();

            return ApiResponse<CourseListItemDto>.Ok(MapToCourseListItemDto(course), "Cập nhật khóa học thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi cập nhật khóa học {Id}", id);
            return ApiResponse<CourseListItemDto>.Error("Đã xảy ra lỗi khi cập nhật khóa học");
        }
    }

    public async Task<ApiResponse<object?>> DeleteCourseAsync(int id)
    {
        try
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
                return ApiResponse<object?>.NotFound("Khóa học không tồn tại");

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();

            return ApiResponse<object?>.Ok(null, "Xóa khóa học thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xóa khóa học {Id}", id);
            return ApiResponse<object?>.Error("Đã xảy ra lỗi khi xóa khóa học");
        }
    }

    private static CourseListItemDto MapToCourseListItemDto(Course course) => new()
    {
        Id = course.Id,
        Title = course.Title,
        Thumbnail = course.Thumbnail,
        Price = course.Price,
        Status = course.Status.ToString(),
        CreatedAt = course.CreatedAt
    };

    private static CourseDetailDto MapToCourseDetailDto(Course course) => new()
    {
        Id = course.Id,
        Title = course.Title,
        Description = course.Description,
        Thumbnail = course.Thumbnail,
        Price = course.Price,
        Status = course.Status.ToString(),
        CreatedAt = course.CreatedAt,
        Sections = course.Sections.Select(s => new SectionDetailDto
        {
            Id = s.Id,
            CourseId = s.CourseId,
            Title = s.Title,
            Position = s.Position,
            Lessons = s.Lessons.Select(l => new LessonResponseDto
            {
                Id = l.Id,
                SectionId = l.SectionId,
                Title = l.Title,
                Content = l.Content,
                VideoUrl = l.VideoUrl,
                LessonType = l.LessonType.ToString(),
                Position = l.Position
            }).ToList()
        }).ToList()
    };
}
