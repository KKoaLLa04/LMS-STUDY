namespace Backend.Models;

// Nhóm quyền tái sử dụng được — Admin định nghĩa 1 lần (vd. "Giáo viên khóa học"), rồi gán cho
// nhiều giáo viên cùng lúc thay vì tick từng module cho từng người. Một giáo viên có thể thuộc
// nhiều nhóm; quyền hiệu lực = OR giữa TeacherModulePermission (override riêng, xem
// TeacherModulePermission.cs) và mọi PermissionGroupModulePermission của các nhóm đang gán —
// xem TeacherPermissionService.HasPermissionAsync.
public class PermissionGroup
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class PermissionGroupModulePermission
{
    public int Id { get; set; }
    public int GroupId { get; set; }
    public PermissionModule Module { get; set; }
    public bool CanView { get; set; }
    public bool CanCreate { get; set; }
    public bool CanUpdate { get; set; }
    public bool CanDelete { get; set; }
}

// Quan hệ nhiều-nhiều User (Teacher) <-> PermissionGroup.
public class UserPermissionGroup
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int GroupId { get; set; }
}
