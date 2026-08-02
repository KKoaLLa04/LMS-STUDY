using Backend.Common;
using Backend.Data;
using Backend.DTOs;
using Backend.Models;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _context;
    private readonly ITeacherPermissionService _teacherPermissionService;
    private readonly ILogger<UserService> _logger;

    public UserService(AppDbContext context, ITeacherPermissionService teacherPermissionService, ILogger<UserService> logger)
    {
        _context = context;
        _teacherPermissionService = teacherPermissionService;
        _logger = logger;
    }

    public async Task<ApiResponse<List<UserDto>>> GetByRoleAsync(UserRole? role)
    {
        try
        {
            var query = _context.Users.AsQueryable();
            if (role.HasValue)
                query = query.Where(u => u.Role == role.Value);

            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            var items = await MapToDtosAsync(users);

            return ApiResponse<List<UserDto>>.Ok(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách người dùng");
            return ApiResponse<List<UserDto>>.Error("Đã xảy ra lỗi khi lấy danh sách người dùng");
        }
    }

    public async Task<ApiResponse<PagedResultDto<UserDto>>> GetPagedAsync(UserRole? role, int page, int pageSize, string? keyword)
    {
        try
        {
            var query = _context.Users.AsQueryable();
            if (role.HasValue)
                query = query.Where(u => u.Role == role.Value);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.Trim();
                query = query.Where(u =>
                    u.FullName.Contains(kw) ||
                    u.Email.Contains(kw) ||
                    u.Username.Contains(kw) ||
                    u.Phone.Contains(kw));
            }

            var totalCount = await query.CountAsync();

            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = await MapToDtosAsync(users);

            return ApiResponse<PagedResultDto<UserDto>>.Ok(new PagedResultDto<UserDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách người dùng (phân trang)");
            return ApiResponse<PagedResultDto<UserDto>>.Error("Đã xảy ra lỗi khi lấy danh sách người dùng");
        }
    }

    public async Task<ApiResponse<UserDto>> GetByIdAsync(int id)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
                return ApiResponse<UserDto>.NotFound("Không tìm thấy người dùng");

            return ApiResponse<UserDto>.Ok(await MapToDtoAsync(user));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy chi tiết người dùng {Id}", id);
            return ApiResponse<UserDto>.Error("Đã xảy ra lỗi khi lấy thông tin người dùng");
        }
    }

    public async Task<ApiResponse<UserDto>> CreateAsync(CreateUserDto dto, bool callerIsAdmin)
    {
        try
        {
            var username = dto.Username.Trim();
            var email = dto.Email.Trim();

            if (await _context.Users.AnyAsync(u => u.Username == username))
                return ApiResponse<UserDto>.BadRequest("Tên đăng nhập đã tồn tại");

            if (await _context.Users.AnyAsync(u => u.Email == email))
                return ApiResponse<UserDto>.BadRequest("Email đã tồn tại");

            var user = new User
            {
                Username = username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Email = email,
                FullName = dto.FullName.Trim(),
                Phone = dto.Phone.Trim(),
                AvatarUrl = string.IsNullOrWhiteSpace(dto.AvatarUrl) ? null : dto.AvatarUrl.Trim(),
                Status = dto.Status,
                Gender = dto.Gender,
                DateOfBirth = dto.DateOfBirth,
                Address = string.IsNullOrWhiteSpace(dto.Address) ? null : dto.Address.Trim(),
                Subject = string.IsNullOrWhiteSpace(dto.Subject) ? null : dto.Subject.Trim(),
                ExperienceYears = dto.ExperienceYears,
                Bio = string.IsNullOrWhiteSpace(dto.Bio) ? null : dto.Bio.Trim(),
                KhoiHocId = dto.KhoiHocId,
                Role = dto.Role,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Chỉ Admin mới được đặt quyền module, và chỉ có ý nghĩa với tài khoản Teacher.
            if (callerIsAdmin && dto.Role == UserRole.Teacher && dto.Permissions != null)
                await _teacherPermissionService.SetForUserAsync(user.Id, dto.Permissions);

            return ApiResponse<UserDto>.Ok(await MapToDtoAsync(user), "Tạo tài khoản thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tạo người dùng");
            return ApiResponse<UserDto>.Error("Đã xảy ra lỗi khi tạo tài khoản");
        }
    }

    public async Task<ApiResponse<UserDto>> UpdateAsync(int id, UpdateUserDto dto, bool callerIsAdmin)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
                return ApiResponse<UserDto>.NotFound("Không tìm thấy người dùng");

            var username = dto.Username.Trim();
            var email = dto.Email.Trim();

            if (await _context.Users.AnyAsync(u => u.Username == username && u.Id != id))
                return ApiResponse<UserDto>.BadRequest("Tên đăng nhập đã tồn tại");

            if (await _context.Users.AnyAsync(u => u.Email == email && u.Id != id))
                return ApiResponse<UserDto>.BadRequest("Email đã tồn tại");

            user.Username = username;
            user.Email = email;
            user.FullName = dto.FullName.Trim();
            user.Phone = dto.Phone.Trim();
            user.AvatarUrl = string.IsNullOrWhiteSpace(dto.AvatarUrl) ? null : dto.AvatarUrl.Trim();
            user.Status = dto.Status;
            user.Gender = dto.Gender;
            user.DateOfBirth = dto.DateOfBirth;
            user.Address = string.IsNullOrWhiteSpace(dto.Address) ? null : dto.Address.Trim();
            user.Subject = string.IsNullOrWhiteSpace(dto.Subject) ? null : dto.Subject.Trim();
            user.ExperienceYears = dto.ExperienceYears;
            user.Bio = string.IsNullOrWhiteSpace(dto.Bio) ? null : dto.Bio.Trim();
            user.KhoiHocId = dto.KhoiHocId;
            user.Role = dto.Role;
            if (!string.IsNullOrWhiteSpace(dto.Password))
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            await _context.SaveChangesAsync();

            // Chỉ Admin mới được đặt quyền module, và chỉ có ý nghĩa với tài khoản Teacher.
            if (callerIsAdmin && dto.Role == UserRole.Teacher && dto.Permissions != null)
                await _teacherPermissionService.SetForUserAsync(user.Id, dto.Permissions);

            return ApiResponse<UserDto>.Ok(await MapToDtoAsync(user), "Cập nhật tài khoản thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi cập nhật người dùng {Id}", id);
            return ApiResponse<UserDto>.Error("Đã xảy ra lỗi khi cập nhật tài khoản");
        }
    }

    public async Task<ApiResponse<object?>> DeleteAsync(int id)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
                return ApiResponse<object?>.NotFound("Không tìm thấy người dùng");

            if (user.Role == UserRole.Admin)
                return ApiResponse<object?>.BadRequest("Không thể xóa tài khoản quản trị viên");

            user.IsDeleted = true;
            user.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return ApiResponse<object?>.Ok(null, "Xóa tài khoản thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xóa người dùng {Id}", id);
            return ApiResponse<object?>.Error("Đã xảy ra lỗi khi xóa tài khoản");
        }
    }

    public async Task<ApiResponse<List<PublicUserDto>>> GetPublicTeachersAsync()
    {
        try
        {
            var items = await _context.Users
                .Where(u => u.Role == UserRole.Teacher)
                .OrderBy(u => u.FullName)
                .Select(u => MapToPublicDto(u))
                .ToListAsync();

            return ApiResponse<List<PublicUserDto>>.Ok(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách giáo viên công khai");
            return ApiResponse<List<PublicUserDto>>.Error("Đã xảy ra lỗi khi lấy danh sách giáo viên");
        }
    }

    public async Task<ApiResponse<PublicUserDto>> GetPublicByIdAsync(int id)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
                return ApiResponse<PublicUserDto>.NotFound("Không tìm thấy người dùng");

            return ApiResponse<PublicUserDto>.Ok(MapToPublicDto(user));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy thông tin công khai người dùng {Id}", id);
            return ApiResponse<PublicUserDto>.Error("Đã xảy ra lỗi khi lấy thông tin người dùng");
        }
    }

    private static PublicUserDto MapToPublicDto(User user) => new()
    {
        Id = user.Id,
        FullName = user.FullName,
        AvatarUrl = user.AvatarUrl,
        Role = user.Role.ToString(),
        Gender = user.Gender?.ToString(),
        Subject = user.Subject,
        ExperienceYears = user.ExperienceYears,
        Bio = user.Bio
    };

    private static UserDto MapToDto(User user, List<TeacherPermissionDto> permissions, List<string> groupNames) => new()
    {
        Id = user.Id,
        Username = user.Username,
        Email = user.Email,
        FullName = user.FullName,
        Phone = user.Phone,
        AvatarUrl = user.AvatarUrl,
        Status = user.Status.ToString(),
        Gender = user.Gender?.ToString(),
        DateOfBirth = user.DateOfBirth,
        Address = user.Address,
        Subject = user.Subject,
        ExperienceYears = user.ExperienceYears,
        Bio = user.Bio,
        KhoiHocId = user.KhoiHocId,
        Role = user.Role.ToString(),
        CreatedAt = user.CreatedAt,
        Permissions = permissions,
        PermissionGroupNames = groupNames
    };

    private async Task<UserDto> MapToDtoAsync(User user)
    {
        if (user.Role != UserRole.Teacher)
            return MapToDto(user, [], []);

        var permissions = await _teacherPermissionService.GetForUserAsync(user.Id);
        var groupNames = await (
            from ug in _context.UserPermissionGroups.AsNoTracking()
            join g in _context.PermissionGroups.AsNoTracking() on ug.GroupId equals g.Id
            where ug.UserId == user.Id
            select g.Name
        ).ToListAsync();

        return MapToDto(user, permissions, groupNames);
    }

    // Nạp quyền + nhóm quyền cho toàn bộ giáo viên trong danh sách bằng vài truy vấn duy nhất
    // (tránh N+1) rồi gộp lại theo UserId — số lượng giáo viên trong một trường thường nhỏ nên
    // gộp trong bộ nhớ là đủ.
    private async Task<List<UserDto>> MapToDtosAsync(List<User> users)
    {
        var teacherIds = users.Where(u => u.Role == UserRole.Teacher).Select(u => u.Id).ToList();

        var permissionsByUser = teacherIds.Count == 0
            ? new Dictionary<int, List<TeacherPermissionDto>>()
            : (await _context.TeacherModulePermissions
                .AsNoTracking()
                .Where(p => teacherIds.Contains(p.UserId))
                .ToListAsync())
              .GroupBy(p => p.UserId)
              .ToDictionary(g => g.Key, g => g.Select(p => new TeacherPermissionDto
              {
                  Module = p.Module,
                  CanView = p.CanView,
                  CanCreate = p.CanCreate,
                  CanUpdate = p.CanUpdate,
                  CanDelete = p.CanDelete
              }).ToList());

        var groupNamesByUser = teacherIds.Count == 0
            ? new Dictionary<int, List<string>>()
            : (await (
                from ug in _context.UserPermissionGroups.AsNoTracking()
                join g in _context.PermissionGroups.AsNoTracking() on ug.GroupId equals g.Id
                where teacherIds.Contains(ug.UserId)
                select new { ug.UserId, g.Name }
              ).ToListAsync())
              .GroupBy(x => x.UserId)
              .ToDictionary(g => g.Key, g => g.Select(x => x.Name).ToList());

        return users
            .Select(u => MapToDto(
                u,
                permissionsByUser.TryGetValue(u.Id, out var perms) ? perms : [],
                groupNamesByUser.TryGetValue(u.Id, out var names) ? names : []))
            .ToList();
    }
}
