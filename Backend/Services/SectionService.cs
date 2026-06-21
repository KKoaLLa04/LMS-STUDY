using Backend.Common;
using Backend.Data;
using Backend.DTOs;
using Backend.Models;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class SectionService : ISectionService
{
    private readonly AppDbContext _context;
    private readonly ILogger<SectionService> _logger;

    public SectionService(AppDbContext context, ILogger<SectionService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<SectionDetailDto>> CreateSectionAsync(CreateSectionDto dto)
    {
        try
        {
            var courseExists = await _context.Courses.AnyAsync(c => c.Id == dto.CourseId);
            if (!courseExists)
                return ApiResponse<SectionDetailDto>.NotFound("Khóa học không tồn tại");

            var section = new Section
            {
                CourseId = dto.CourseId,
                Title = dto.Title.Trim(),
                Position = dto.Position
            };

            _context.Sections.Add(section);
            await _context.SaveChangesAsync();

            return ApiResponse<SectionDetailDto>.Ok(MapToSectionDetailDto(section), "Tạo chương học thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tạo chương học");
            return ApiResponse<SectionDetailDto>.Error("Đã xảy ra lỗi khi tạo chương học");
        }
    }

    public async Task<ApiResponse<SectionDetailDto>> UpdateSectionAsync(int id, UpdateSectionDto dto)
    {
        try
        {
            var section = await _context.Sections
                .Include(s => s.Lessons.OrderBy(l => l.Position))
                .FirstOrDefaultAsync(s => s.Id == id);

            if (section == null)
                return ApiResponse<SectionDetailDto>.NotFound("Chương học không tồn tại");

            section.Title = dto.Title.Trim();
            section.Position = dto.Position;

            await _context.SaveChangesAsync();

            return ApiResponse<SectionDetailDto>.Ok(MapToSectionDetailDto(section), "Cập nhật chương học thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi cập nhật chương học {Id}", id);
            return ApiResponse<SectionDetailDto>.Error("Đã xảy ra lỗi khi cập nhật chương học");
        }
    }

    public async Task<ApiResponse<object?>> DeleteSectionAsync(int id)
    {
        try
        {
            var section = await _context.Sections.FindAsync(id);
            if (section == null)
                return ApiResponse<object?>.NotFound("Chương học không tồn tại");

            _context.Sections.Remove(section);
            await _context.SaveChangesAsync();

            return ApiResponse<object?>.Ok(null, "Xóa chương học thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xóa chương học {Id}", id);
            return ApiResponse<object?>.Error("Đã xảy ra lỗi khi xóa chương học");
        }
    }

    private static SectionDetailDto MapToSectionDetailDto(Section section) => new()
    {
        Id = section.Id,
        CourseId = section.CourseId,
        Title = section.Title,
        Position = section.Position,
        Lessons = section.Lessons.Select(l => new LessonResponseDto
        {
            Id = l.Id,
            SectionId = l.SectionId,
            Title = l.Title,
            Content = l.Content,
            VideoUrl = l.VideoUrl,
            LessonType = l.LessonType.ToString(),
            Position = l.Position
        }).ToList()
    };
}
