using Backend.Common;
using Backend.Data;
using Backend.DTOs;
using Backend.Models;
using Backend.Services.VirtualClassrooms.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.VirtualClassrooms;

public class ChatChannelService : IChatChannelService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ChatChannelService> _logger;

    public ChatChannelService(AppDbContext context, ILogger<ChatChannelService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<List<ChatChannelResponseDto>>> GetChannelsByCourseAsync(int courseId)
    {
        try
        {
            var channels = await _context.ChatChannels
                .Include(c => c.Course)
                .Include(c => c.Messages)
                .Where(c => c.CourseId == courseId)
                .OrderBy(c => c.Name)
                .Select(c => MapToResponseDto(c))
                .ToListAsync();

            return ApiResponse<List<ChatChannelResponseDto>>.Ok(channels);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách kênh chat cho khóa học {CourseId}", courseId);
            return ApiResponse<List<ChatChannelResponseDto>>.Error("Đã xảy ra lỗi khi lấy danh sách kênh chat");
        }
    }

    public async Task<ApiResponse<ChatChannelResponseDto>> GetChannelByIdAsync(int id)
    {
        try
        {
            var channel = await _context.ChatChannels
                .Include(c => c.Course)
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (channel is null)
                return ApiResponse<ChatChannelResponseDto>.NotFound("Không tìm thấy kênh chat");

            return ApiResponse<ChatChannelResponseDto>.Ok(MapToResponseDto(channel));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy kênh chat {Id}", id);
            return ApiResponse<ChatChannelResponseDto>.Error("Đã xảy ra lỗi khi lấy kênh chat");
        }
    }

    public async Task<ApiResponse<ChatChannelResponseDto>> CreateChannelAsync(CreateChatChannelDto dto)
    {
        try
        {
            var courseExists = await _context.Courses.AnyAsync(c => c.Id == dto.CourseId);
            if (!courseExists)
                return ApiResponse<ChatChannelResponseDto>.NotFound("Không tìm thấy khóa học");

            var duplicate = await _context.ChatChannels
                .AnyAsync(c => c.CourseId == dto.CourseId && c.Name == dto.Name);
            if (duplicate)
                return ApiResponse<ChatChannelResponseDto>.BadRequest("Kênh chat với tên này đã tồn tại trong khóa học");

            var channel = new ChatChannel
            {
                Name = dto.Name,
                Description = dto.Description,
                CourseId = dto.CourseId
            };

            _context.ChatChannels.Add(channel);
            await _context.SaveChangesAsync();

            await _context.Entry(channel).Reference(c => c.Course).LoadAsync();

            return ApiResponse<ChatChannelResponseDto>.Ok(MapToResponseDto(channel), "Tạo kênh chat thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tạo kênh chat");
            return ApiResponse<ChatChannelResponseDto>.Error("Đã xảy ra lỗi khi tạo kênh chat");
        }
    }

    public async Task<ApiResponse<ChatChannelResponseDto>> UpdateChannelAsync(int id, UpdateChatChannelDto dto)
    {
        try
        {
            var channel = await _context.ChatChannels
                .Include(c => c.Course)
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (channel is null)
                return ApiResponse<ChatChannelResponseDto>.NotFound("Không tìm thấy kênh chat");

            channel.Name = dto.Name;
            channel.Description = dto.Description;

            await _context.SaveChangesAsync();

            return ApiResponse<ChatChannelResponseDto>.Ok(MapToResponseDto(channel), "Cập nhật kênh chat thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi cập nhật kênh chat {Id}", id);
            return ApiResponse<ChatChannelResponseDto>.Error("Đã xảy ra lỗi khi cập nhật kênh chat");
        }
    }

    public async Task<ApiResponse<object?>> DeleteChannelAsync(int id)
    {
        try
        {
            var channel = await _context.ChatChannels.FindAsync(id);
            if (channel is null)
                return ApiResponse<object?>.NotFound("Không tìm thấy kênh chat");

            _context.ChatChannels.Remove(channel);
            await _context.SaveChangesAsync();

            return ApiResponse<object?>.Ok(null, "Xóa kênh chat thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xóa kênh chat {Id}", id);
            return ApiResponse<object?>.Error("Đã xảy ra lỗi khi xóa kênh chat");
        }
    }

    public async Task<ApiResponse<PagedResultDto<ChatMessageResponseDto>>> GetMessagesAsync(int channelId, int page, int pageSize)
    {
        try
        {
            var channelExists = await _context.ChatChannels.AnyAsync(c => c.Id == channelId);
            if (!channelExists)
                return ApiResponse<PagedResultDto<ChatMessageResponseDto>>.NotFound("Không tìm thấy kênh chat");

            var query = _context.ChatMessages
                .Where(m => m.ChannelId == channelId)
                .OrderByDescending(m => m.SentAt);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new ChatMessageResponseDto
                {
                    Id = m.Id,
                    Content = m.Content,
                    SenderName = m.SenderName,
                    ChannelId = m.ChannelId,
                    SentAt = m.SentAt
                })
                .ToListAsync();

            var result = new PagedResultDto<ChatMessageResponseDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };

            return ApiResponse<PagedResultDto<ChatMessageResponseDto>>.Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy tin nhắn của kênh {ChannelId}", channelId);
            return ApiResponse<PagedResultDto<ChatMessageResponseDto>>.Error("Đã xảy ra lỗi khi lấy tin nhắn");
        }
    }

    public async Task<ApiResponse<ChatMessageResponseDto>> SendMessageAsync(int channelId, SendMessageDto dto)
    {
        try
        {
            var channelExists = await _context.ChatChannels.AnyAsync(c => c.Id == channelId);
            if (!channelExists)
                return ApiResponse<ChatMessageResponseDto>.NotFound("Không tìm thấy kênh chat");

            var message = new ChatMessage
            {
                Content = dto.Content,
                SenderName = dto.SenderName,
                ChannelId = channelId
            };

            _context.ChatMessages.Add(message);
            await _context.SaveChangesAsync();

            return ApiResponse<ChatMessageResponseDto>.Ok(new ChatMessageResponseDto
            {
                Id = message.Id,
                Content = message.Content,
                SenderName = message.SenderName,
                ChannelId = message.ChannelId,
                SentAt = message.SentAt
            }, "Gửi tin nhắn thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi gửi tin nhắn vào kênh {ChannelId}", channelId);
            return ApiResponse<ChatMessageResponseDto>.Error("Đã xảy ra lỗi khi gửi tin nhắn");
        }
    }

    private static ChatChannelResponseDto MapToResponseDto(ChatChannel c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        Description = c.Description,
        CourseId = c.CourseId,
        CourseName = c.Course?.Title ?? string.Empty,
        CreatedAt = c.CreatedAt,
        MessageCount = c.Messages?.Count ?? 0
    };
}
