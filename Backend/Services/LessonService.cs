using Backend.Common;
using Backend.Data;
using Backend.DTOs;
using Backend.Models;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class LessonService : ILessonService
{
    private readonly AppDbContext _context;
    private readonly ILogger<LessonService> _logger;

    public LessonService(AppDbContext context, ILogger<LessonService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<LessonResponseDto>> CreateLessonAsync(CreateLessonDto dto)
    {
        try
        {
            var sectionExists = await _context.Sections.AnyAsync(s => s.Id == dto.SectionId);
            if (!sectionExists)
                return ApiResponse<LessonResponseDto>.NotFound("Chương học không tồn tại");

            if (!Enum.TryParse<LessonType>(dto.LessonType, ignoreCase: true, out var lessonType))
                return ApiResponse<LessonResponseDto>.BadRequest(
                    "Loại bài học không hợp lệ. Chỉ chấp nhận: Video hoặc Document");

            var lesson = new Lesson
            {
                SectionId = dto.SectionId,
                Title = dto.Title.Trim(),
                Content = dto.Content?.Trim(),
                VideoUrl = dto.VideoUrl?.Trim(),
                LessonType = lessonType,
                Position = dto.Position
            };

            _context.Lessons.Add(lesson);
            await _context.SaveChangesAsync();

            return ApiResponse<LessonResponseDto>.Ok(MapToLessonResponseDto(lesson), "Tạo bài học thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tạo bài học");
            return ApiResponse<LessonResponseDto>.Error("Đã xảy ra lỗi khi tạo bài học");
        }
    }

    public async Task<ApiResponse<LessonResponseDto>> UpdateLessonAsync(int id, UpdateLessonDto dto)
    {
        try
        {
            var lesson = await _context.Lessons.FindAsync(id);
            if (lesson == null)
                return ApiResponse<LessonResponseDto>.NotFound("Bài học không tồn tại");

            if (!Enum.TryParse<LessonType>(dto.LessonType, ignoreCase: true, out var lessonType))
                return ApiResponse<LessonResponseDto>.BadRequest(
                    "Loại bài học không hợp lệ. Chỉ chấp nhận: Video hoặc Document");

            lesson.Title = dto.Title.Trim();
            lesson.Content = dto.Content?.Trim();
            lesson.VideoUrl = dto.VideoUrl?.Trim();
            lesson.LessonType = lessonType;
            lesson.Position = dto.Position;

            await _context.SaveChangesAsync();

            return ApiResponse<LessonResponseDto>.Ok(MapToLessonResponseDto(lesson), "Cập nhật bài học thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi cập nhật bài học {Id}", id);
            return ApiResponse<LessonResponseDto>.Error("Đã xảy ra lỗi khi cập nhật bài học");
        }
    }

    public async Task<ApiResponse<object?>> DeleteLessonAsync(int id)
    {
        try
        {
            var lesson = await _context.Lessons.FindAsync(id);
            if (lesson == null)
                return ApiResponse<object?>.NotFound("Bài học không tồn tại");

            _context.Lessons.Remove(lesson);
            await _context.SaveChangesAsync();

            return ApiResponse<object?>.Ok(null, "Xóa bài học thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xóa bài học {Id}", id);
            return ApiResponse<object?>.Error("Đã xảy ra lỗi khi xóa bài học");
        }
    }

    private static LessonResponseDto MapToLessonResponseDto(Lesson lesson) => new()
    {
        Id = lesson.Id,
        SectionId = lesson.SectionId,
        Title = lesson.Title,
        Content = lesson.Content,
        VideoUrl = lesson.VideoUrl,
        LessonType = lesson.LessonType.ToString(),
        Position = lesson.Position
    };
}
