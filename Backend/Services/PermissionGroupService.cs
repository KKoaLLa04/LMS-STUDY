using Backend.Common;
using Backend.Data;
using Backend.DTOs;
using Backend.Models;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class PermissionGroupService : IPermissionGroupService
{
    private readonly AppDbContext _context;
    private readonly ILogger<PermissionGroupService> _logger;

    public PermissionGroupService(AppDbContext context, ILogger<PermissionGroupService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<List<PermissionGroupDto>>> GetAllAsync()
    {
        try
        {
            var memberCounts = await _context.UserPermissionGroups
                .AsNoTracking()
                .GroupBy(ug => ug.GroupId)
                .Select(g => new { GroupId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.GroupId, x => x.Count);

            var groups = await _context.PermissionGroups
                .AsNoTracking()
                .OrderBy(g => g.Name)
                .ToListAsync();

            var items = groups.Select(g => new PermissionGroupDto
            {
                Id = g.Id,
                Name = g.Name,
                Description = g.Description,
                MemberCount = memberCounts.TryGetValue(g.Id, out var count) ? count : 0
            }).ToList();

            return ApiResponse<List<PermissionGroupDto>>.Ok(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách nhóm quyền");
            return ApiResponse<List<PermissionGroupDto>>.Error("Đã xảy ra lỗi khi lấy danh sách nhóm quyền");
        }
    }

    public async Task<ApiResponse<PermissionGroupDetailDto>> GetByIdAsync(int id)
    {
        try
        {
            var group = await _context.PermissionGroups.AsNoTracking().FirstOrDefaultAsync(g => g.Id == id);
            if (group == null)
                return ApiResponse<PermissionGroupDetailDto>.NotFound("Không tìm thấy nhóm quyền");

            return ApiResponse<PermissionGroupDetailDto>.Ok(await MapToDetailDtoAsync(group));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy chi tiết nhóm quyền {Id}", id);
            return ApiResponse<PermissionGroupDetailDto>.Error("Đã xảy ra lỗi khi lấy thông tin nhóm quyền");
        }
    }

    public async Task<ApiResponse<PermissionGroupDetailDto>> CreateAsync(CreatePermissionGroupDto dto)
    {
        try
        {
            var name = dto.Name.Trim();
            if (await _context.PermissionGroups.AnyAsync(g => g.Name == name))
                return ApiResponse<PermissionGroupDetailDto>.BadRequest("Tên nhóm quyền đã tồn tại");

            var group = new PermissionGroup
            {
                Name = name,
                Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim()
            };
            _context.PermissionGroups.Add(group);
            await _context.SaveChangesAsync();

            await SaveModulePermissionsAsync(group.Id, dto.ModulePermissions);

            return ApiResponse<PermissionGroupDetailDto>.Ok(await MapToDetailDtoAsync(group), "Tạo nhóm quyền thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tạo nhóm quyền");
            return ApiResponse<PermissionGroupDetailDto>.Error("Đã xảy ra lỗi khi tạo nhóm quyền");
        }
    }

    public async Task<ApiResponse<PermissionGroupDetailDto>> UpdateAsync(int id, UpdatePermissionGroupDto dto)
    {
        try
        {
            var group = await _context.PermissionGroups.FirstOrDefaultAsync(g => g.Id == id);
            if (group == null)
                return ApiResponse<PermissionGroupDetailDto>.NotFound("Không tìm thấy nhóm quyền");

            var name = dto.Name.Trim();
            if (await _context.PermissionGroups.AnyAsync(g => g.Name == name && g.Id != id))
                return ApiResponse<PermissionGroupDetailDto>.BadRequest("Tên nhóm quyền đã tồn tại");

            group.Name = name;
            group.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
            await _context.SaveChangesAsync();

            await SaveModulePermissionsAsync(group.Id, dto.ModulePermissions);

            return ApiResponse<PermissionGroupDetailDto>.Ok(await MapToDetailDtoAsync(group), "Cập nhật nhóm quyền thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi cập nhật nhóm quyền {Id}", id);
            return ApiResponse<PermissionGroupDetailDto>.Error("Đã xảy ra lỗi khi cập nhật nhóm quyền");
        }
    }

    public async Task<ApiResponse<object?>> DeleteAsync(int id)
    {
        try
        {
            var group = await _context.PermissionGroups.FirstOrDefaultAsync(g => g.Id == id);
            if (group == null)
                return ApiResponse<object?>.NotFound("Không tìm thấy nhóm quyền");

            // Cascade delete (cấu hình ở AppDbContext) tự dọn PermissionGroupModulePermissions và
            // UserPermissionGroups liên quan — giáo viên trong nhóm chỉ mất phần quyền do nhóm này
            // cấp, override riêng của họ không bị ảnh hưởng.
            _context.PermissionGroups.Remove(group);
            await _context.SaveChangesAsync();

            return ApiResponse<object?>.Ok(null, "Xóa nhóm quyền thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xóa nhóm quyền {Id}", id);
            return ApiResponse<object?>.Error("Đã xảy ra lỗi khi xóa nhóm quyền");
        }
    }

    public async Task<ApiResponse<object?>> SetMembersAsync(int groupId, List<int> teacherUserIds)
    {
        try
        {
            var group = await _context.PermissionGroups.FirstOrDefaultAsync(g => g.Id == groupId);
            if (group == null)
                return ApiResponse<object?>.NotFound("Không tìm thấy nhóm quyền");

            var distinctIds = teacherUserIds.Distinct().ToList();
            var validTeacherCount = await _context.Users
                .CountAsync(u => distinctIds.Contains(u.Id) && u.Role == UserRole.Teacher);
            if (validTeacherCount != distinctIds.Count)
                return ApiResponse<object?>.BadRequest("Chỉ có thể thêm tài khoản giáo viên vào nhóm quyền");

            var existing = await _context.UserPermissionGroups.Where(ug => ug.GroupId == groupId).ToListAsync();
            _context.UserPermissionGroups.RemoveRange(existing);
            _context.UserPermissionGroups.AddRange(distinctIds.Select(userId => new UserPermissionGroup
            {
                GroupId = groupId,
                UserId = userId
            }));
            await _context.SaveChangesAsync();

            return ApiResponse<object?>.Ok(null, "Cập nhật thành viên nhóm quyền thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi cập nhật thành viên nhóm quyền {Id}", groupId);
            return ApiResponse<object?>.Error("Đã xảy ra lỗi khi cập nhật thành viên nhóm quyền");
        }
    }

    private async Task SaveModulePermissionsAsync(int groupId, List<TeacherPermissionDto> permissions)
    {
        var existing = await _context.PermissionGroupModulePermissions
            .Where(p => p.GroupId == groupId)
            .ToListAsync();
        _context.PermissionGroupModulePermissions.RemoveRange(existing);

        var toAdd = permissions
            .Where(p => p.CanView || p.CanCreate || p.CanUpdate || p.CanDelete)
            .Select(p => new PermissionGroupModulePermission
            {
                GroupId = groupId,
                Module = p.Module,
                CanView = p.CanView,
                CanCreate = p.CanCreate,
                CanUpdate = p.CanUpdate,
                CanDelete = p.CanDelete
            });
        _context.PermissionGroupModulePermissions.AddRange(toAdd);

        await _context.SaveChangesAsync();
    }

    private async Task<PermissionGroupDetailDto> MapToDetailDtoAsync(PermissionGroup permissionGroup)
    {
        var modulePermissions = await _context.PermissionGroupModulePermissions
            .AsNoTracking()
            .Where(p => p.GroupId == permissionGroup.Id)
            .Select(p => new TeacherPermissionDto
            {
                Module = p.Module,
                CanView = p.CanView,
                CanCreate = p.CanCreate,
                CanUpdate = p.CanUpdate,
                CanDelete = p.CanDelete
            })
            .ToListAsync();

        var members = await (
            from ug in _context.UserPermissionGroups.AsNoTracking()
            join u in _context.Users.AsNoTracking() on ug.UserId equals u.Id
            where ug.GroupId == permissionGroup.Id
            orderby u.FullName
            select new PermissionGroupMemberDto { Id = u.Id, Username = u.Username, FullName = u.FullName }
        ).ToListAsync();

        return new PermissionGroupDetailDto
        {
            Id = permissionGroup.Id,
            Name = permissionGroup.Name,
            Description = permissionGroup.Description,
            ModulePermissions = modulePermissions,
            Members = members
        };
    }
}
