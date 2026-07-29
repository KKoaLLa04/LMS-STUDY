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
                    "Loại bài học không hợp lệ. Chỉ chấp nhận: Video, Document hoặc Quiz");

            var lesson = new Lesson
            {
                SectionId = dto.SectionId,
                Title = dto.Title.Trim(),
                Content = dto.Content?.Trim(),
                VideoUrl = dto.VideoUrl?.Trim(),
                DocumentUrl = dto.DocumentUrl?.Trim(),
                LessonType = lessonType,
                Position = dto.Position,
                DurationMinutes = dto.DurationMinutes
            };

            var linkError = await ApplySharedLinkAsync(lesson, lessonType, dto.DocumentId, dto.QuizId,
                dto.Content, dto.DocumentUrl, dto.Title, isNewLesson: true);
            if (linkError != null)
                return ApiResponse<LessonResponseDto>.BadRequest(linkError);

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
            var lesson = await _context.Lessons.FirstOrDefaultAsync(l => l.Id == id);
            if (lesson == null)
                return ApiResponse<LessonResponseDto>.NotFound("Bài học không tồn tại");

            if (!Enum.TryParse<LessonType>(dto.LessonType, ignoreCase: true, out var lessonType))
                return ApiResponse<LessonResponseDto>.BadRequest(
                    "Loại bài học không hợp lệ. Chỉ chấp nhận: Video, Document hoặc Quiz");

            lesson.Title = dto.Title.Trim();
            lesson.Content = dto.Content?.Trim();
            lesson.VideoUrl = dto.VideoUrl?.Trim();
            lesson.DocumentUrl = dto.DocumentUrl?.Trim();
            lesson.LessonType = lessonType;
            lesson.Position = dto.Position;
            lesson.DurationMinutes = dto.DurationMinutes;

            var linkError = await ApplySharedLinkAsync(lesson, lessonType, dto.DocumentId, dto.QuizId,
                dto.Content, dto.DocumentUrl, dto.Title, isNewLesson: false);
            if (linkError != null)
                return ApiResponse<LessonResponseDto>.BadRequest(linkError);

            await _context.SaveChangesAsync();

            return ApiResponse<LessonResponseDto>.Ok(MapToLessonResponseDto(lesson), "Cập nhật bài học thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi cập nhật bài học {Id}", id);
            return ApiResponse<LessonResponseDto>.Error("Đã xảy ra lỗi khi cập nhật bài học");
        }
    }

    // Gắn Lesson với Document/Quiz dùng chung: nếu admin chọn DocumentId/QuizId có sẵn (từ kho
    // chung) thì chỉ trỏ FK sang đó; nếu không chọn, tự tạo (lần đầu) hoặc cập nhật tại chỗ (khi
    // sửa lại) một Document/Quiz "riêng" cho bài học này — vẫn là một mục hợp lệ trong kho chung,
    // chỉ là chưa được lesson nào khác dùng lại.
    private async Task<string?> ApplySharedLinkAsync(
        Lesson lesson, LessonType lessonType, int? documentId, int? quizId,
        string? content, string? documentUrl, string title, bool isNewLesson)
    {
        if (lessonType == LessonType.Document)
        {
            if (documentId.HasValue)
            {
                var exists = await _context.Documents.AnyAsync(d => d.Id == documentId.Value);
                if (!exists) return "Tài liệu chung được chọn không tồn tại";
                lesson.DocumentId = documentId.Value;
            }
            else if (!string.IsNullOrWhiteSpace(content) || !string.IsNullOrWhiteSpace(documentUrl))
            {
                if (!isNewLesson && lesson.DocumentId.HasValue)
                {
                    var doc = await _context.Documents.FirstOrDefaultAsync(d => d.Id == lesson.DocumentId.Value);
                    if (doc != null)
                    {
                        doc.Title = title.Trim();
                        doc.Content = content?.Trim();
                        doc.FileUrl = documentUrl?.Trim();
                        doc.UpdatedAt = DateTime.UtcNow;
                        return null;
                    }
                }

                var newDoc = new Document
                {
                    Title = title.Trim(),
                    Content = content?.Trim(),
                    FileUrl = documentUrl?.Trim(),
                    CreatedAt = DateTime.UtcNow
                };
                _context.Documents.Add(newDoc);
                lesson.Document = newDoc;
            }
        }
        else if (lessonType == LessonType.Quiz)
        {
            if (quizId.HasValue)
            {
                var exists = await _context.Quizzes.AnyAsync(q => q.Id == quizId.Value);
                if (!exists) return "Quiz chung được chọn không tồn tại";
                lesson.QuizId = quizId.Value;
            }
            else if (isNewLesson || !lesson.QuizId.HasValue)
            {
                var newQuiz = new Quiz { Title = title.Trim(), CreatedAt = DateTime.UtcNow };
                _context.Quizzes.Add(newQuiz);
                lesson.Quiz = newQuiz;
            }
            else
            {
                var quiz = await _context.Quizzes.FirstOrDefaultAsync(q => q.Id == lesson.QuizId!.Value);
                if (quiz != null)
                {
                    quiz.Title = title.Trim();
                    quiz.UpdatedAt = DateTime.UtcNow;
                }
            }
        }

        return null;
    }

    public async Task<ApiResponse<object?>> DeleteLessonAsync(int id)
    {
        try
        {
            var lesson = await _context.Lessons.FirstOrDefaultAsync(l => l.Id == id);
            if (lesson == null)
                return ApiResponse<object?>.NotFound("Bài học không tồn tại");

            lesson.IsDeleted = true;
            lesson.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return ApiResponse<object?>.Ok(null, "Xóa bài học thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xóa bài học {Id}", id);
            return ApiResponse<object?>.Error("Đã xảy ra lỗi khi xóa bài học");
        }
    }

    public async Task<ApiResponse<LessonResponseDto>> GetByIdAsync(int id)
    {
        try
        {
            var lesson = await _context.Lessons.FirstOrDefaultAsync(l => l.Id == id);
            if (lesson == null)
                return ApiResponse<LessonResponseDto>.NotFound("Bài học không tồn tại");

            return ApiResponse<LessonResponseDto>.Ok(MapToLessonResponseDto(lesson));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy chi tiết bài học {Id}", id);
            return ApiResponse<LessonResponseDto>.Error("Đã xảy ra lỗi khi lấy thông tin bài học");
        }
    }

    public async Task<ApiResponse<List<LessonAdminListItemDto>>> GetByTypeAsync(string lessonType)
    {
        try
        {
            if (!Enum.TryParse<LessonType>(lessonType, ignoreCase: true, out var type))
                return ApiResponse<List<LessonAdminListItemDto>>.BadRequest(
                    "Loại bài học không hợp lệ. Chỉ chấp nhận: Video, Document hoặc Quiz");

            var items = await _context.Lessons
                .Include(l => l.Section).ThenInclude(s => s.Course)
                .Where(l => l.LessonType == type)
                .OrderByDescending(l => l.Id)
                .Select(l => new LessonAdminListItemDto
                {
                    Id = l.Id,
                    Title = l.Title,
                    LessonType = l.LessonType.ToString(),
                    Content = l.Content,
                    VideoUrl = l.VideoUrl,
                    DocumentUrl = l.DocumentUrl,
                    Position = l.Position,
                    DurationMinutes = l.DurationMinutes,
                    SectionId = l.SectionId,
                    SectionTitle = l.Section.Title,
                    CourseId = l.Section.CourseId,
                    CourseTitle = l.Section.Course.Title,
                    QuestionCount = l.QuizId == null ? 0 : _context.QuizQuestions.Count(q => q.QuizId == l.QuizId)
                })
                .ToListAsync();

            return ApiResponse<List<LessonAdminListItemDto>>.Ok(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách bài học theo loại {LessonType}", lessonType);
            return ApiResponse<List<LessonAdminListItemDto>>.Error("Đã xảy ra lỗi khi lấy danh sách bài học");
        }
    }

    private static LessonResponseDto MapToLessonResponseDto(Lesson lesson) => new()
    {
        Id = lesson.Id,
        SectionId = lesson.SectionId,
        Title = lesson.Title,
        Content = lesson.Content,
        VideoUrl = lesson.VideoUrl,
        DocumentUrl = lesson.DocumentUrl,
        LessonType = lesson.LessonType.ToString(),
        Position = lesson.Position,
        DurationMinutes = lesson.DurationMinutes,
        DocumentId = lesson.DocumentId,
        QuizId = lesson.QuizId
    };
}
