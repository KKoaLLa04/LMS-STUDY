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
        int page, int pageSize, string? keyword, bool isAdmin)
    {
        try
        {
            // Không có navigation property/FK giữa Course và KhoiHoc/CourseCategory, nên join
            // thủ công bằng LINQ (join theo Id) thay vì Include — các Id này chỉ là giá trị thường.
            var query =
                from c in _context.Courses
                join k in _context.KhoiHocs on c.KhoiHocId equals k.Id into khoiJoin
                from k in khoiJoin.DefaultIfEmpty()
                join cat in _context.CourseCategories on c.CategoryId equals cat.Id into catJoin
                from cat in catJoin.DefaultIfEmpty()
                select new { Course = c, KhoiHoc = k, Category = cat };

            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(x => x.Course.Title.Contains(keyword));

            // Học sinh (không phải Admin) chỉ được thấy khóa học đã Published —
            // tránh lộ khóa học Draft/Upcoming ra khu vực client.
            if (!isAdmin)
                query = query.Where(x => x.Course.Status == CourseStatus.Published);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.Course.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new CourseListItemDto
                {
                    Id = x.Course.Id,
                    Title = x.Course.Title,
                    Thumbnail = x.Course.Thumbnail,
                    Teacher = x.Course.Teacher,
                    Emoji = x.Course.Emoji,
                    Price = x.Course.Price,
                    Status = x.Course.Status.ToString(),
                    CreatedAt = x.Course.CreatedAt,
                    KhoiHocId = x.Course.KhoiHocId,
                    KhoiHocName = x.KhoiHoc != null ? x.KhoiHoc.Name : null,
                    CategoryId = x.Course.CategoryId,
                    CategoryName = x.Category != null ? x.Category.Name : null,
                    IsFeatured = x.Course.IsFeatured,
                    LessonsCount = _context.Lessons.Count(l => l.Section.CourseId == x.Course.Id),
                    DurationMinutes = _context.Lessons.Where(l => l.Section.CourseId == x.Course.Id).Sum(l => (int?)l.DurationMinutes) ?? 0
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

    public async Task<ApiResponse<CourseDetailDto>> GetCourseByIdAsync(int id, bool isAdmin)
    {
        try
        {
            var course = await _context.Courses
                .Include(c => c.Sections.OrderBy(s => s.Position))
                    .ThenInclude(s => s.Lessons.OrderBy(l => l.Position))
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
                return ApiResponse<CourseDetailDto>.NotFound("Khóa học không tồn tại");

            // Học sinh không được xem khóa học chưa Published — coi như không tồn tại.
            if (!isAdmin && course.Status != CourseStatus.Published)
                return ApiResponse<CourseDetailDto>.NotFound("Khóa học không tồn tại");

            string? khoiHocName = course.KhoiHocId.HasValue
                ? await _context.KhoiHocs
                    .Where(k => k.Id == course.KhoiHocId.Value)
                    .Select(k => k.Name)
                    .FirstOrDefaultAsync()
                : null;

            string? categoryName = course.CategoryId.HasValue
                ? await _context.CourseCategories
                    .Where(c => c.Id == course.CategoryId.Value)
                    .Select(c => c.Name)
                    .FirstOrDefaultAsync()
                : null;

            var dto = MapToCourseDetailDto(course, khoiHocName, categoryName);
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
                    "Trạng thái không hợp lệ. Chỉ chấp nhận: Draft, Published hoặc Upcoming");

            var course = new Course
            {
                Title = dto.Title.Trim(),
                Description = dto.Description?.Trim(),
                Thumbnail = dto.Thumbnail?.Trim(),
                Teacher = dto.Teacher?.Trim(),
                Emoji = dto.Emoji?.Trim(),
                Price = dto.Price,
                Status = status,
                KhoiHocId = dto.KhoiHocId,
                CategoryId = dto.CategoryId,
                IsFeatured = dto.IsFeatured,
                PreviewVideoUrl = dto.PreviewVideoUrl?.Trim()
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
            var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == id);
            if (course == null)
                return ApiResponse<CourseListItemDto>.NotFound("Khóa học không tồn tại");

            if (!Enum.TryParse<CourseStatus>(dto.Status, ignoreCase: true, out var status))
                return ApiResponse<CourseListItemDto>.BadRequest(
                    "Trạng thái không hợp lệ. Chỉ chấp nhận: Draft, Published hoặc Upcoming");

            course.Title = dto.Title.Trim();
            course.Description = dto.Description?.Trim();
            course.Thumbnail = dto.Thumbnail?.Trim();
            course.Teacher = dto.Teacher?.Trim();
            course.Emoji = dto.Emoji?.Trim();
            course.Price = dto.Price;
            course.Status = status;
            course.KhoiHocId = dto.KhoiHocId;
            course.CategoryId = dto.CategoryId;
            course.IsFeatured = dto.IsFeatured;
            course.PreviewVideoUrl = dto.PreviewVideoUrl?.Trim();

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
            var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == id);
            if (course == null)
                return ApiResponse<object?>.NotFound("Khóa học không tồn tại");

            var deletedAt = DateTime.UtcNow;

            // Xóa mềm toàn bộ dữ liệu con — trước đây các bảng này cascade xóa cứng theo Course,
            // nay phải tự cascade xóa mềm để không còn dữ liệu "mồ côi" hiển thị dưới khóa học đã xóa.
            var sectionIds = await _context.Sections
                .Where(s => s.CourseId == id)
                .Select(s => s.Id)
                .ToListAsync();

            if (sectionIds.Count > 0)
            {
                await _context.Lessons
                    .Where(l => sectionIds.Contains(l.SectionId))
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(x => x.IsDeleted, true)
                        .SetProperty(x => x.DeletedAt, deletedAt));
            }

            await _context.Sections
                .Where(s => s.CourseId == id)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.IsDeleted, true)
                    .SetProperty(x => x.DeletedAt, deletedAt));

            await _context.VirtualClassrooms
                .Where(v => v.CourseId == id)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.IsDeleted, true)
                    .SetProperty(x => x.DeletedAt, deletedAt));

            await _context.DiscussionPosts
                .Where(p => p.CourseId == id)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.IsDeleted, true)
                    .SetProperty(x => x.DeletedAt, deletedAt));

            var channelIds = await _context.ChatChannels
                .Where(c => c.CourseId == id)
                .Select(c => c.Id)
                .ToListAsync();

            if (channelIds.Count > 0)
            {
                await _context.ChatMessages
                    .Where(m => channelIds.Contains(m.ChannelId))
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(x => x.IsDeleted, true)
                        .SetProperty(x => x.DeletedAt, deletedAt));
            }

            await _context.ChatChannels
                .Where(c => c.CourseId == id)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.IsDeleted, true)
                    .SetProperty(x => x.DeletedAt, deletedAt));

            course.IsDeleted = true;
            course.DeletedAt = deletedAt;
            await _context.SaveChangesAsync();

            return ApiResponse<object?>.Ok(null, "Xóa khóa học thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xóa khóa học {Id}", id);
            return ApiResponse<object?>.Error("Đã xảy ra lỗi khi xóa khóa học");
        }
    }

    private static CourseListItemDto MapToCourseListItemDto(Course course, string? khoiHocName = null, string? categoryName = null) => new()
    {
        Id = course.Id,
        Title = course.Title,
        Thumbnail = course.Thumbnail,
        Teacher = course.Teacher,
        Emoji = course.Emoji,
        Price = course.Price,
        Status = course.Status.ToString(),
        CreatedAt = course.CreatedAt,
        KhoiHocId = course.KhoiHocId,
        KhoiHocName = khoiHocName,
        CategoryId = course.CategoryId,
        CategoryName = categoryName,
        IsFeatured = course.IsFeatured,
        LessonsCount = 0,
        DurationMinutes = 0
    };

    private static CourseDetailDto MapToCourseDetailDto(Course course, string? khoiHocName = null, string? categoryName = null) => new()
    {
        Id = course.Id,
        Title = course.Title,
        Description = course.Description,
        Thumbnail = course.Thumbnail,
        Teacher = course.Teacher,
        Emoji = course.Emoji,
        Price = course.Price,
        Status = course.Status.ToString(),
        CreatedAt = course.CreatedAt,
        KhoiHocId = course.KhoiHocId,
        KhoiHocName = khoiHocName,
        CategoryId = course.CategoryId,
        CategoryName = categoryName,
        IsFeatured = course.IsFeatured,
        PreviewVideoUrl = course.PreviewVideoUrl,
        LessonsCount = course.Sections.Sum(s => s.Lessons.Count),
        DurationMinutes = course.Sections.Sum(s => s.Lessons.Sum(l => l.DurationMinutes)),
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
                DocumentUrl = l.DocumentUrl,
                LessonType = l.LessonType.ToString(),
                Position = l.Position,
                DurationMinutes = l.DurationMinutes
            }).ToList()
        }).ToList()
    };
}
