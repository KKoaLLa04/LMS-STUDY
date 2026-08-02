using Backend.Data;
using Backend.DTOs;
using Backend.Models;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class TeacherPermissionService : ITeacherPermissionService
{
    private readonly AppDbContext _context;

    public TeacherPermissionService(AppDbContext context)
    {
        _context = context;
    }

    // Quyền hiệu lực = OR giữa override riêng của giáo viên (TeacherModulePermissions) và mọi
    // nhóm quyền giáo viên đang là thành viên (PermissionGroupModulePermissions qua
    // UserPermissionGroups) — chỉ cần MỘT trong hai nguồn cho phép là đủ.
    public async Task<bool> HasPermissionAsync(int userId, PermissionModule module, PermissionAction action)
    {
        var ownPermission = await _context.TeacherModulePermissions
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId && p.Module == module);

        if (ownPermission != null && HasAction(ownPermission.CanView, ownPermission.CanCreate, ownPermission.CanUpdate, ownPermission.CanDelete, action))
            return true;

        var groupGrants = await (
            from ug in _context.UserPermissionGroups.AsNoTracking()
            join gp in _context.PermissionGroupModulePermissions.AsNoTracking()
                on ug.GroupId equals gp.GroupId
            where ug.UserId == userId && gp.Module == module
            select gp
        ).ToListAsync();

        return groupGrants.Any(gp => HasAction(gp.CanView, gp.CanCreate, gp.CanUpdate, gp.CanDelete, action));
    }

    public async Task<List<TeacherPermissionDto>> GetForUserAsync(int userId)
    {
        return await _context.TeacherModulePermissions
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .Select(p => MapToDto(p))
            .ToListAsync();
    }

    // Danh sách quyền đã gộp (override OR mọi nhóm) — dùng để gửi cho Frontend (ẩn/hiện menu,
    // route, nút bấm), khác với GetForUserAsync (chỉ override riêng, dùng để hiển thị lại lưới
    // checkbox trong form Edit Teacher).
    public async Task<List<TeacherPermissionDto>> GetEffectivePermissionsAsync(int userId)
    {
        var own = await _context.TeacherModulePermissions
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .ToListAsync();

        var groupPermissions = await (
            from ug in _context.UserPermissionGroups.AsNoTracking()
            join gp in _context.PermissionGroupModulePermissions.AsNoTracking()
                on ug.GroupId equals gp.GroupId
            where ug.UserId == userId
            select gp
        ).ToListAsync();

        var merged = new Dictionary<PermissionModule, TeacherPermissionDto>();
        void Merge(PermissionModule module, bool canView, bool canCreate, bool canUpdate, bool canDelete)
        {
            if (!merged.TryGetValue(module, out var dto))
            {
                dto = new TeacherPermissionDto { Module = module };
                merged[module] = dto;
            }
            dto.CanView |= canView;
            dto.CanCreate |= canCreate;
            dto.CanUpdate |= canUpdate;
            dto.CanDelete |= canDelete;
        }

        foreach (var p in own)
            Merge(p.Module, p.CanView, p.CanCreate, p.CanUpdate, p.CanDelete);
        foreach (var gp in groupPermissions)
            Merge(gp.Module, gp.CanView, gp.CanCreate, gp.CanUpdate, gp.CanDelete);

        return merged.Values
            .Where(d => d.CanView || d.CanCreate || d.CanUpdate || d.CanDelete)
            .ToList();
    }

    private static bool HasAction(bool canView, bool canCreate, bool canUpdate, bool canDelete, PermissionAction action) =>
        action switch
        {
            PermissionAction.View => canView,
            PermissionAction.Create => canCreate,
            PermissionAction.Update => canUpdate,
            PermissionAction.Delete => canDelete,
            _ => false
        };

    public async Task SetForUserAsync(int userId, List<TeacherPermissionDto> permissions)
    {
        var existing = await _context.TeacherModulePermissions
            .Where(p => p.UserId == userId)
            .ToListAsync();
        _context.TeacherModulePermissions.RemoveRange(existing);

        // Chỉ lưu module thật sự có ít nhất một quyền bật — tránh rác dữ liệu toàn false
        // (không có bản ghi = không có quyền, hai trạng thái này tương đương nhau).
        var toAdd = permissions
            .Where(p => p.CanView || p.CanCreate || p.CanUpdate || p.CanDelete)
            .Select(p => new TeacherModulePermission
            {
                UserId = userId,
                Module = p.Module,
                CanView = p.CanView,
                CanCreate = p.CanCreate,
                CanUpdate = p.CanUpdate,
                CanDelete = p.CanDelete
            });
        _context.TeacherModulePermissions.AddRange(toAdd);

        await _context.SaveChangesAsync();
    }

    private static TeacherPermissionDto MapToDto(TeacherModulePermission p) => new()
    {
        Module = p.Module,
        CanView = p.CanView,
        CanCreate = p.CanCreate,
        CanUpdate = p.CanUpdate,
        CanDelete = p.CanDelete
    };
}
