using Backend.Common;
using Backend.DTOs;

namespace Backend.Services.VirtualClassrooms.Interfaces;

public interface IChatChannelService
{
    Task<ApiResponse<List<ChatChannelResponseDto>>> GetChannelsByCourseAsync(int courseId);
    Task<ApiResponse<ChatChannelResponseDto>> GetChannelByIdAsync(int id);
    Task<ApiResponse<ChatChannelResponseDto>> CreateChannelAsync(CreateChatChannelDto dto);
    Task<ApiResponse<ChatChannelResponseDto>> UpdateChannelAsync(int id, UpdateChatChannelDto dto);
    Task<ApiResponse<object?>> DeleteChannelAsync(int id);
    Task<ApiResponse<PagedResultDto<ChatMessageResponseDto>>> GetMessagesAsync(int channelId, int page, int pageSize);
    Task<ApiResponse<ChatMessageResponseDto>> SendMessageAsync(int channelId, SendMessageDto dto);
}
