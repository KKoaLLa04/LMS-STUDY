using Backend.Common;
using Backend.Data;
using Backend.DTOs;
using Backend.Models;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class DocumentService : IDocumentService
{
    private readonly AppDbContext _context;
    private readonly ILogger<DocumentService> _logger;

    public DocumentService(AppDbContext context, ILogger<DocumentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<List<DocumentResponseDto>>> GetAllAsync()
    {
        try
        {
            var items = await _context.Documents
                .OrderByDescending(d => d.Id)
                .Select(d => new DocumentResponseDto
                {
                    Id = d.Id,
                    Title = d.Title,
                    Content = d.Content,
                    FileUrl = d.FileUrl,
                    CreatedAt = d.CreatedAt,
                    UpdatedAt = d.UpdatedAt,
                    LinkedLessonCount = _context.Lessons.Count(l => l.DocumentId == d.Id)
                })
                .ToListAsync();

            return ApiResponse<List<DocumentResponseDto>>.Ok(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách tài liệu chung");
            return ApiResponse<List<DocumentResponseDto>>.Error("Đã xảy ra lỗi khi lấy danh sách tài liệu");
        }
    }

    public async Task<ApiResponse<DocumentResponseDto>> GetByIdAsync(int id)
    {
        try
        {
            var document = await _context.Documents.FirstOrDefaultAsync(d => d.Id == id);
            if (document == null)
                return ApiResponse<DocumentResponseDto>.NotFound("Không tìm thấy tài liệu");

            var linkedCount = await _context.Lessons.CountAsync(l => l.DocumentId == id);
            return ApiResponse<DocumentResponseDto>.Ok(MapToResponseDto(document, linkedCount));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy chi tiết tài liệu {Id}", id);
            return ApiResponse<DocumentResponseDto>.Error("Đã xảy ra lỗi khi lấy thông tin tài liệu");
        }
    }

    public async Task<ApiResponse<DocumentResponseDto>> CreateAsync(CreateDocumentDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Content) && string.IsNullOrWhiteSpace(dto.FileUrl))
                return ApiResponse<DocumentResponseDto>.BadRequest("Cần nhập nội dung hoặc tải lên file tài liệu");

            var document = new Document
            {
                Title = dto.Title.Trim(),
                Content = dto.Content?.Trim(),
                FileUrl = dto.FileUrl?.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _context.Documents.Add(document);
            await _context.SaveChangesAsync();

            return ApiResponse<DocumentResponseDto>.Ok(MapToResponseDto(document, 0), "Tạo tài liệu thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tạo tài liệu chung");
            return ApiResponse<DocumentResponseDto>.Error("Đã xảy ra lỗi khi tạo tài liệu");
        }
    }

    public async Task<ApiResponse<DocumentResponseDto>> UpdateAsync(int id, UpdateDocumentDto dto)
    {
        try
        {
            var document = await _context.Documents.FirstOrDefaultAsync(d => d.Id == id);
            if (document == null)
                return ApiResponse<DocumentResponseDto>.NotFound("Không tìm thấy tài liệu");

            if (string.IsNullOrWhiteSpace(dto.Content) && string.IsNullOrWhiteSpace(dto.FileUrl))
                return ApiResponse<DocumentResponseDto>.BadRequest("Cần nhập nội dung hoặc tải lên file tài liệu");

            document.Title = dto.Title.Trim();
            document.Content = dto.Content?.Trim();
            document.FileUrl = dto.FileUrl?.Trim();
            document.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var linkedCount = await _context.Lessons.CountAsync(l => l.DocumentId == id);
            return ApiResponse<DocumentResponseDto>.Ok(MapToResponseDto(document, linkedCount), "Cập nhật tài liệu thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi cập nhật tài liệu {Id}", id);
            return ApiResponse<DocumentResponseDto>.Error("Đã xảy ra lỗi khi cập nhật tài liệu");
        }
    }

    public async Task<ApiResponse<object?>> DeleteAsync(int id)
    {
        try
        {
            var document = await _context.Documents.FirstOrDefaultAsync(d => d.Id == id);
            if (document == null)
                return ApiResponse<object?>.NotFound("Không tìm thấy tài liệu");

            // Xóa mềm: không chặn dù tài liệu đang gắn ở bài học nào đó — giống cách xử lý
            // KhoiHocId/CategoryId trên Course (giá trị thường, không FK), để tránh việc xóa
            // bị kẹt vì các tham chiếu khác. Các bài học đã gắn vẫn giữ DocumentId cũ.
            document.IsDeleted = true;
            document.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return ApiResponse<object?>.Ok(null, "Xóa tài liệu thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xóa tài liệu {Id}", id);
            return ApiResponse<object?>.Error("Đã xảy ra lỗi khi xóa tài liệu");
        }
    }

    public async Task<ApiResponse<List<StudentDocumentDto>>> GetAllForStudentAsync()
    {
        try
        {
            var items = await _context.Documents
                .OrderByDescending(d => d.Id)
                .Select(d => new StudentDocumentDto { Id = d.Id, Title = d.Title, Content = d.Content, FileUrl = d.FileUrl })
                .ToListAsync();

            return ApiResponse<List<StudentDocumentDto>>.Ok(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách tài liệu chung cho học viên");
            return ApiResponse<List<StudentDocumentDto>>.Error("Đã xảy ra lỗi khi lấy danh sách tài liệu");
        }
    }

    public async Task<ApiResponse<StudentDocumentDto>> GetForStudentAsync(int id)
    {
        try
        {
            var document = await _context.Documents.FirstOrDefaultAsync(d => d.Id == id);
            if (document == null)
                return ApiResponse<StudentDocumentDto>.NotFound("Không tìm thấy tài liệu");

            return ApiResponse<StudentDocumentDto>.Ok(new StudentDocumentDto
            {
                Id = document.Id,
                Title = document.Title,
                Content = document.Content,
                FileUrl = document.FileUrl
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy tài liệu {Id} cho học viên", id);
            return ApiResponse<StudentDocumentDto>.Error("Đã xảy ra lỗi khi lấy thông tin tài liệu");
        }
    }

    private static DocumentResponseDto MapToResponseDto(Document d, int linkedLessonCount) => new()
    {
        Id = d.Id,
        Title = d.Title,
        Content = d.Content,
        FileUrl = d.FileUrl,
        CreatedAt = d.CreatedAt,
        UpdatedAt = d.UpdatedAt,
        LinkedLessonCount = linkedLessonCount
    };
}
