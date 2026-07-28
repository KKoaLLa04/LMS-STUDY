using System.ComponentModel.DataAnnotations;
using Backend.Common;

namespace Backend.Models;

public class DiscussionPost : ISoftDelete
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;

    [MaxLength(100)]
    public string AuthorName { get; set; } = string.Empty;

    // Nullable vì các bài viết cũ (hoặc tạo bởi luồng chưa đăng nhập) không có user thật gắn kèm.
    // Dùng để quy điểm tương tác đúng cho học sinh — AuthorName chỉ là tên hiển thị.
    public int? UserId { get; set; }
    public User? User { get; set; }

    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;

    public int? ParentPostId { get; set; }
    public DiscussionPost? ParentPost { get; set; }
    public ICollection<DiscussionPost> Replies { get; set; } = new List<DiscussionPost>();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}
