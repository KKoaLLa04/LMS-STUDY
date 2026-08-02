using Backend.DTOs;
using Backend.Models;

namespace Backend.Services.Interfaces;

public interface ITeacherPermissionService
{
    Task<bool> HasPermissionAsync(int userId, PermissionModule module, PermissionAction action);
    Task<List<TeacherPermissionDto>> GetForUserAsync(int userId);

    // Quyền đã gộp giữa override riêng và mọi nhóm quyền đang gán — dùng cho FE (ẩn/hiện UI),
    // khác GetForUserAsync (chỉ override riêng, dùng để hiển thị lại form Edit Teacher).
    Task<List<TeacherPermissionDto>> GetEffectivePermissionsAsync(int userId);

    // Thay toàn bộ danh sách quyền của một giáo viên (xóa các bản ghi cũ, ghi lại theo danh sách
    // mới) — đơn giản hơn upsert từng dòng vì form Edit User luôn gửi lại toàn bộ danh sách module.
    Task SetForUserAsync(int userId, List<TeacherPermissionDto> permissions);
}
