using Backend.Common;
using Backend.Data;
using Backend.DTOs;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(AppDbContext context, ITokenService tokenService, ILogger<AuthService> logger)
    {
        _context = context;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginDto dto)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == dto.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return ApiResponse<LoginResponseDto>.Unauthorized("Tên đăng nhập hoặc mật khẩu không đúng");

            var (token, expiresAt) = _tokenService.GenerateToken(user);

            var result = new LoginResponseDto
            {
                Token = token,
                ExpiresAt = expiresAt,
                Username = user.Username,
                Role = user.Role.ToString()
            };

            return ApiResponse<LoginResponseDto>.Ok(result, "Đăng nhập thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi đăng nhập");
            return ApiResponse<LoginResponseDto>.Error("Đã xảy ra lỗi khi đăng nhập");
        }
    }

    public async Task<ApiResponse<CurrentUserDto>> GetCurrentUserAsync(int userId)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return ApiResponse<CurrentUserDto>.NotFound("Không tìm thấy người dùng");

            var result = new CurrentUserDto
            {
                Id = user.Id,
                Username = user.Username,
                Role = user.Role.ToString(),
                CreatedAt = user.CreatedAt
            };

            return ApiResponse<CurrentUserDto>.Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy thông tin người dùng hiện tại");
            return ApiResponse<CurrentUserDto>.Error("Đã xảy ra lỗi khi lấy thông tin người dùng");
        }
    }
}
