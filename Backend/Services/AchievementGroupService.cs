using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Backend.Common;
using Backend.Data;
using Backend.DTOs;
using Backend.Models;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class AchievementGroupService : IAchievementGroupService
{
    private readonly AppDbContext _context;
    private readonly ILogger<AchievementGroupService> _logger;

    public AchievementGroupService(AppDbContext context, ILogger<AchievementGroupService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<List<AchievementGroupDto>>> GetAllAsync()
    {
        try
        {
            var items = await _context.AchievementGroups
                .OrderBy(g => g.OrderNumber)
                .Select(g => MapToDto(g))
                .ToListAsync();

            return ApiResponse<List<AchievementGroupDto>>.Ok(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách nhóm huy hiệu");
            return ApiResponse<List<AchievementGroupDto>>.Error("Đã xảy ra lỗi khi lấy danh sách nhóm huy hiệu");
        }
    }

    public async Task<ApiResponse<AchievementGroupDto>> CreateAsync(CreateAchievementGroupDto dto)
    {
        try
        {
            var name = dto.Name.Trim();
            var code = await BuildUniqueCodeAsync(name);
            var orderNumber = await _context.AchievementGroups.CountAsync();

            var group = new AchievementGroup
            {
                Name = name,
                Code = code,
                OrderNumber = orderNumber
            };

            _context.AchievementGroups.Add(group);
            await _context.SaveChangesAsync();

            return ApiResponse<AchievementGroupDto>.Ok(MapToDto(group), "Tạo nhóm huy hiệu thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tạo nhóm huy hiệu");
            return ApiResponse<AchievementGroupDto>.Error("Đã xảy ra lỗi khi tạo nhóm huy hiệu");
        }
    }

    // Sinh mã ổn định từ tên nhóm (bỏ dấu, chỉ giữ chữ/số) — Admin không cần nhập tay, chỉ dùng
    // nội bộ để AchievementDto.Category giữ tương thích ngược với enum Category cũ.
    private async Task<string> BuildUniqueCodeAsync(string name)
    {
        var baseCode = Slugify(name);
        var code = baseCode;
        var suffix = 2;

        while (await _context.AchievementGroups.AnyAsync(g => g.Code == code))
        {
            var suffixText = suffix.ToString();
            code = baseCode.Length + suffixText.Length > 50
                ? baseCode[..(50 - suffixText.Length)] + suffixText
                : baseCode + suffixText;
            suffix++;
        }

        return code;
    }

    private static string Slugify(string name)
    {
        var normalized = name.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }

        var ascii = sb.ToString().Replace('Đ', 'D').Replace('đ', 'd');
        var cleaned = Regex.Replace(ascii, "[^a-zA-Z0-9]+", "");
        if (cleaned.Length == 0) cleaned = "Nhom";

        return cleaned.Length > 50 ? cleaned[..50] : cleaned;
    }

    private static AchievementGroupDto MapToDto(AchievementGroup group) => new()
    {
        Id = group.Id,
        Name = group.Name,
        OrderNumber = group.OrderNumber
    };
}
