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
    private readonly ILogger<UserService> _logger;

    public UserService(AppDbContext context, ILogger<UserService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<List<UserDto>>> GetByRoleAsync(UserRole? role)
    {
        try
        {
            var query = _context.Users.AsQueryable();
            if (role.HasValue)
                query = query.Where(u => u.Role == role.Value);

            var items = await query
                .OrderByDescending(u => u.CreatedAt)
                .Select(u => MapToDto(u))
                .ToListAsync();

            return ApiResponse<List<UserDto>>.Ok(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách người dùng");
            return ApiResponse<List<UserDto>>.Error("Đã xảy ra lỗi khi lấy danh sách người dùng");
        }
    }

    public async Task<ApiResponse<UserDto>> GetByIdAsync(int id)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
                return ApiResponse<UserDto>.NotFound("Không tìm thấy người dùng");

            return ApiResponse<UserDto>.Ok(MapToDto(user));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy chi tiết người dùng {Id}", id);
            return ApiResponse<UserDto>.Error("Đã xảy ra lỗi khi lấy thông tin người dùng");
        }
    }

    public async Task<ApiResponse<UserDto>> CreateAsync(CreateUserDto dto)
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

            return ApiResponse<UserDto>.Ok(MapToDto(user), "Tạo tài khoản thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tạo người dùng");
            return ApiResponse<UserDto>.Error("Đã xảy ra lỗi khi tạo tài khoản");
        }
    }

    public async Task<ApiResponse<UserDto>> UpdateAsync(int id, UpdateUserDto dto)
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

            return ApiResponse<UserDto>.Ok(MapToDto(user), "Cập nhật tài khoản thành công");
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

    private static UserDto MapToDto(User user) => new()
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
        CreatedAt = user.CreatedAt
    };
}
