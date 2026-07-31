using Backend.Common;
using Backend.Data;
using Backend.DTOs;
using Backend.Models;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

// Số liệu tổng quan toàn nền tảng — chỉ trả về số đếm (không lộ danh sách/PII), dùng cho khối
// thống kê ở Trang chủ (thay cho số liệu "120,000+..." trước đây bị hardcode).
public class PlatformStatsService : IPlatformStatsService
{
    private readonly AppDbContext _context;
    private readonly ILogger<PlatformStatsService> _logger;

    public PlatformStatsService(AppDbContext context, ILogger<PlatformStatsService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<PlatformStatsDto>> GetStatsAsync()
    {
        try
        {
            var studentsCount = await _context.Users.CountAsync(u => u.Role == UserRole.Student);
            var teachersCount = await _context.Users.CountAsync(u => u.Role == UserRole.Teacher);
            var coursesCount = await _context.Courses.CountAsync(c => c.Status == CourseStatus.Published);
            var contentMinutes = await _context.Lessons
                .Where(l => l.Section.Course.Status == CourseStatus.Published)
                .SumAsync(l => (int?)l.DurationMinutes) ?? 0;

            var dto = new PlatformStatsDto
            {
                StudentsCount = studentsCount,
                TeachersCount = teachersCount,
                CoursesCount = coursesCount,
                ContentHours = contentMinutes / 60
            };

            return ApiResponse<PlatformStatsDto>.Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy số liệu tổng quan nền tảng");
            return ApiResponse<PlatformStatsDto>.Error("Đã xảy ra lỗi khi lấy số liệu thống kê");
        }
    }
}
