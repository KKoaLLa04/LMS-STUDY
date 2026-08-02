namespace Backend.Models;

// Danh mục module cố định mà quyền có thể áp dụng — xem can-hoan-thien-truoc-golive.md.
// Quản lý tài khoản Teacher (UsersController role=Teacher) và Uploads cố tình không nằm trong
// danh sách này (không thể phân quyền), tránh giáo viên tự nâng quyền cho chính mình/đồng nghiệp.
public enum PermissionModule
{
    Courses,
    CourseCategories,
    KhoiHocs,
    Achievements,
    Documents,
    Quizzes,
    Students,
    DiscussionForums,
    VirtualClassrooms
}

public enum PermissionAction
{
    View,
    Create,
    Update,
    Delete
}

// Quyền CRUD của một giáo viên trên một module cụ thể. Không có bản ghi cho một (UserId, Module)
// nghĩa là giáo viên đó chưa được cấp quyền gì trên module đó (mặc định đóng, Admin phải cấp
// tường minh) — không cần backfill khi thêm bảng này.
public class TeacherModulePermission
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public PermissionModule Module { get; set; }
    public bool CanView { get; set; }
    public bool CanCreate { get; set; }
    public bool CanUpdate { get; set; }
    public bool CanDelete { get; set; }
}
