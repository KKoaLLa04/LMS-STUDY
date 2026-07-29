using System.ComponentModel.DataAnnotations;
using Backend.Common;

namespace Backend.Models;

// Nhóm huy hiệu do Admin tự tạo — thay cho enum Category cố định (HocTap/ChuyenCan/TuongTac)
// trước đây, dùng làm filter và để phân loại khi tạo huy hiệu mới.
public class AchievementGroup : ISoftDelete
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    // Mã ổn định (không đổi khi Admin đổi tên nhóm) — tự sinh từ Name lúc tạo, dùng để giữ
    // tương thích ngược với Achievement.Category (enum cũ) ở AchievementDto.Category cho các
    // trang client cũ (Frontend/src/app/clients) vẫn còn đọc field "category" dạng chuỗi.
    [Required]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    public int OrderNumber { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}
