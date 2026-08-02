using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs;

public class PermissionGroupDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int MemberCount { get; set; }
}

public class PermissionGroupMemberDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
}

public class PermissionGroupDetailDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<TeacherPermissionDto> ModulePermissions { get; set; } = [];
    public List<PermissionGroupMemberDto> Members { get; set; } = [];
}

public class CreatePermissionGroupDto
{
    [Required(ErrorMessage = "Tên nhóm quyền không được để trống")]
    [MaxLength(255, ErrorMessage = "Tên nhóm quyền không vượt quá 255 ký tự")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000, ErrorMessage = "Mô tả không vượt quá 1000 ký tự")]
    public string? Description { get; set; }

    public List<TeacherPermissionDto> ModulePermissions { get; set; } = [];
}

public class UpdatePermissionGroupDto
{
    [Required(ErrorMessage = "Tên nhóm quyền không được để trống")]
    [MaxLength(255, ErrorMessage = "Tên nhóm quyền không vượt quá 255 ký tự")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000, ErrorMessage = "Mô tả không vượt quá 1000 ký tự")]
    public string? Description { get; set; }

    public List<TeacherPermissionDto> ModulePermissions { get; set; } = [];
}

public class SetPermissionGroupMembersDto
{
    public List<int> UserIds { get; set; } = [];
}
