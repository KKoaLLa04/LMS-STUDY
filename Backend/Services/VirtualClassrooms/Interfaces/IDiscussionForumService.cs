using Backend.Common;
using Backend.DTOs;

namespace Backend.Services.VirtualClassrooms.Interfaces;

public interface IDiscussionForumService
{
    Task<ApiResponse<PagedResultDto<DiscussionPostListItemDto>>> GetPostsByCourseAsync(int courseId, int page, int pageSize, string? keyword);
    Task<ApiResponse<DiscussionPostResponseDto>> GetPostByIdAsync(int id);
    Task<ApiResponse<DiscussionPostResponseDto>> CreatePostAsync(CreateDiscussionPostDto dto);
    Task<ApiResponse<DiscussionPostResponseDto>> UpdatePostAsync(int id, UpdateDiscussionPostDto dto);
    Task<ApiResponse<object?>> DeletePostAsync(int id);
    Task<ApiResponse<DiscussionPostResponseDto>> ReplyToPostAsync(int parentPostId, CreateDiscussionPostDto dto);
}
