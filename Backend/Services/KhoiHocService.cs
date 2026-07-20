using Backend.Common;
using Backend.Data;
using Backend.DTOs;
using Backend.Models;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class KhoiHocService : IKhoiHocService
{
    private readonly AppDbContext _context;
    private readonly ILogger<KhoiHocService> _logger;

    public KhoiHocService(AppDbContext context, ILogger<KhoiHocService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<List<KhoiHocDto>>> GetAllAsync()
    {
        try
        {
            var items = await _context.KhoiHocs
                .OrderBy(k => k.OrderNumber)
                .Select(k => MapToDto(k))
                .ToListAsync();

            return ApiResponse<List<KhoiHocDto>>.Ok(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách khối học");
            return ApiResponse<List<KhoiHocDto>>.Error("Đã xảy ra lỗi khi lấy danh sách khối học");
        }
    }

    public async Task<ApiResponse<KhoiHocDto>> GetByIdAsync(int id)
    {
        try
        {
            var khoiHoc = await _context.KhoiHocs.FirstOrDefaultAsync(k => k.Id == id);
            if (khoiHoc == null)
                return ApiResponse<KhoiHocDto>.NotFound("Khối học không tồn tại");

            return ApiResponse<KhoiHocDto>.Ok(MapToDto(khoiHoc));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy chi tiết khối học {Id}", id);
            return ApiResponse<KhoiHocDto>.Error("Đã xảy ra lỗi khi lấy thông tin khối học");
        }
    }

    public async Task<ApiResponse<KhoiHocDto>> CreateAsync(CreateKhoiHocDto dto)
    {
        try
        {
            var code = dto.Code.Trim();

            var codeExists = await _context.KhoiHocs.AnyAsync(k => k.Code == code);
            if (codeExists)
                return ApiResponse<KhoiHocDto>.BadRequest($"Mã khối học \"{code}\" đã tồn tại");

            var khoiHoc = new KhoiHoc
            {
                Name = dto.Name.Trim(),
                Code = code,
                OrderNumber = dto.OrderNumber
            };

            _context.KhoiHocs.Add(khoiHoc);
            await _context.SaveChangesAsync();

            return ApiResponse<KhoiHocDto>.Ok(MapToDto(khoiHoc), "Tạo khối học thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tạo khối học");
            return ApiResponse<KhoiHocDto>.Error("Đã xảy ra lỗi khi tạo khối học");
        }
    }

    public async Task<ApiResponse<KhoiHocDto>> UpdateAsync(int id, UpdateKhoiHocDto dto)
    {
        try
        {
            var khoiHoc = await _context.KhoiHocs.FirstOrDefaultAsync(k => k.Id == id);
            if (khoiHoc == null)
                return ApiResponse<KhoiHocDto>.NotFound("Khối học không tồn tại");

            var code = dto.Code.Trim();

            var codeExists = await _context.KhoiHocs.AnyAsync(k => k.Code == code && k.Id != id);
            if (codeExists)
                return ApiResponse<KhoiHocDto>.BadRequest($"Mã khối học \"{code}\" đã tồn tại");

            khoiHoc.Name = dto.Name.Trim();
            khoiHoc.Code = code;
            khoiHoc.OrderNumber = dto.OrderNumber;

            await _context.SaveChangesAsync();

            return ApiResponse<KhoiHocDto>.Ok(MapToDto(khoiHoc), "Cập nhật khối học thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi cập nhật khối học {Id}", id);
            return ApiResponse<KhoiHocDto>.Error("Đã xảy ra lỗi khi cập nhật khối học");
        }
    }

    public async Task<ApiResponse<object?>> DeleteAsync(int id)
    {
        try
        {
            var khoiHoc = await _context.KhoiHocs.FirstOrDefaultAsync(k => k.Id == id);
            if (khoiHoc == null)
                return ApiResponse<object?>.NotFound("Khối học không tồn tại");

            khoiHoc.IsDeleted = true;
            khoiHoc.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return ApiResponse<object?>.Ok(null, "Xóa khối học thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xóa khối học {Id}", id);
            return ApiResponse<object?>.Error("Đã xảy ra lỗi khi xóa khối học");
        }
    }

    private static KhoiHocDto MapToDto(KhoiHoc khoiHoc) => new()
    {
        Id = khoiHoc.Id,
        Name = khoiHoc.Name,
        Code = khoiHoc.Code,
        OrderNumber = khoiHoc.OrderNumber
    };
}
