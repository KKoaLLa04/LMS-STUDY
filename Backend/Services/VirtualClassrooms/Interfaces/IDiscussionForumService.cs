using Backend.Common;
using Backend.DTOs;

namespace Backend.Services.VirtualClassrooms.Interfaces;

public interface IDiscussionForumService
{
    Task<ApiResponse<PagedResultDto<DiscussionPostListItemDto>>> GetPostsByCourseAsync(int courseId, int page, int pageSize, string? keyword, int? currentUserId);
    Task<ApiResponse<DiscussionPostResponseDto>> GetPostByIdAsync(int id, int? currentUserId);
    Task<ApiResponse<DiscussionPostResponseDto>> CreatePostAsync(CreateDiscussionPostDto dto, int? currentUserId);
    Task<ApiResponse<DiscussionPostResponseDto>> UpdatePostAsync(int id, UpdateDiscussionPostDto dto);
    Task<ApiResponse<object?>> DeletePostAsync(int id);
    Task<ApiResponse<DiscussionPostResponseDto>> ReplyToPostAsync(int parentPostId, CreateDiscussionPostDto dto, int? currentUserId);
    Task<ApiResponse<object?>> LikePostAsync(int postId, int userId);
    Task<ApiResponse<object?>> UnlikePostAsync(int postId, int userId);
}
