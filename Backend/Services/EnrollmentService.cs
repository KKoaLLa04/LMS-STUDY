using Backend.Common;
using Backend.Data;
using Backend.DTOs;
using Backend.Models;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class EnrollmentService : IEnrollmentService
{
    private readonly AppDbContext _context;
    private readonly ILogger<EnrollmentService> _logger;

    public EnrollmentService(AppDbContext context, ILogger<EnrollmentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<EnrollmentDto>> EnrollAsync(int userId, int courseId)
    {
        try
        {
            var courseExists = await _context.Courses.AnyAsync(c => c.Id == courseId);
            if (!courseExists)
                return ApiResponse<EnrollmentDto>.NotFound("Không tìm thấy khóa học");

            var existing = await _context.Enrollments
                .Include(e => e.Course)
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId);

            if (existing != null)
                return ApiResponse<EnrollmentDto>.Ok(MapToDto(existing), "Học sinh đã ghi danh khóa học này");

            var enrollment = new Enrollment { UserId = userId, CourseId = courseId };
            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            await _context.Entry(enrollment).Reference(e => e.Course).LoadAsync();
            await _context.Entry(enrollment).Reference(e => e.User).LoadAsync();

            return ApiResponse<EnrollmentDto>.Ok(MapToDto(enrollment), "Ghi danh khóa học thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi ghi danh user {UserId} vào khóa học {CourseId}", userId, courseId);
            return ApiResponse<EnrollmentDto>.Error("Đã xảy ra lỗi khi ghi danh khóa học");
        }
    }

    public async Task<ApiResponse<List<EnrollmentDto>>> GetMyEnrollmentsAsync(int userId)
    {
        try
        {
            var items = await _context.Enrollments
                .Include(e => e.Course)
                .Include(e => e.User)
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.EnrolledAt)
                .Select(e => MapToDto(e))
                .ToListAsync();

            return ApiResponse<List<EnrollmentDto>>.Ok(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách khóa học đã ghi danh của user {UserId}", userId);
            return ApiResponse<List<EnrollmentDto>>.Error("Đã xảy ra lỗi khi lấy danh sách khóa học đã ghi danh");
        }
    }

    public async Task<ApiResponse<List<EnrollmentDto>>> GetByCourseAsync(int courseId)
    {
        try
        {
            var items = await _context.Enrollments
                .Include(e => e.Course)
                .Include(e => e.User)
                .Where(e => e.CourseId == courseId)
                .OrderBy(e => e.User.FullName)
                .Select(e => MapToDto(e))
                .ToListAsync();

            return ApiResponse<List<EnrollmentDto>>.Ok(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách học sinh của khóa học {CourseId}", courseId);
            return ApiResponse<List<EnrollmentDto>>.Error("Đã xảy ra lỗi khi lấy danh sách học sinh");
        }
    }

    public async Task<ApiResponse<object?>> UnenrollAsync(int userId, int courseId)
    {
        try
        {
            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId);

            if (enrollment == null)
                return ApiResponse<object?>.NotFound("Không tìm thấy thông tin ghi danh");

            enrollment.IsDeleted = true;
            enrollment.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return ApiResponse<object?>.Ok(null, "Hủy ghi danh thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi hủy ghi danh user {UserId} khỏi khóa học {CourseId}", userId, courseId);
            return ApiResponse<object?>.Error("Đã xảy ra lỗi khi hủy ghi danh khóa học");
        }
    }

    private static EnrollmentDto MapToDto(Enrollment e) => new()
    {
        Id = e.Id,
        UserId = e.UserId,
        StudentName = e.User?.FullName ?? string.Empty,
        CourseId = e.CourseId,
        CourseName = e.Course?.Title ?? string.Empty,
        EnrolledAt = e.EnrolledAt
    };
}
