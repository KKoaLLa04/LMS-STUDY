using Backend.Common;
using Backend.Data;
using Backend.DTOs;
using Backend.Models;
using Backend.Services.MeetingProviders;
using Backend.Services.VirtualClassrooms.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.VirtualClassrooms;

public class VirtualClassroomService : IVirtualClassroomService
{
    private readonly AppDbContext _context;
    private readonly ILogger<VirtualClassroomService> _logger;
    private readonly MeetingProviderFactory _providerFactory;

    public VirtualClassroomService(
        AppDbContext context,
        ILogger<VirtualClassroomService> logger,
        MeetingProviderFactory providerFactory)
    {
        _context = context;
        _logger = logger;
        _providerFactory = providerFactory;
    }

    public async Task<ApiResponse<PagedResultDto<VirtualClassroomResponseDto>>> GetClassroomsByCourseAsync(int courseId, int page, int pageSize)
    {
        try
        {
            var query = _context.VirtualClassrooms
                .Include(v => v.Course)
                .Where(v => v.CourseId == courseId)
                .OrderByDescending(v => v.ScheduledAt);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(v => MapToResponseDto(v))
                .ToListAsync();

            var result = new PagedResultDto<VirtualClassroomResponseDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };

            return ApiResponse<PagedResultDto<VirtualClassroomResponseDto>>.Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách phòng học trực tuyến cho khóa học {CourseId}", courseId);
            return ApiResponse<PagedResultDto<VirtualClassroomResponseDto>>.Error("Đã xảy ra lỗi khi lấy danh sách phòng học");
        }
    }

    public async Task<ApiResponse<VirtualClassroomResponseDto>> GetClassroomByIdAsync(int id)
    {
        try
        {
            var classroom = await _context.VirtualClassrooms
                .Include(v => v.Course)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (classroom is null)
                return ApiResponse<VirtualClassroomResponseDto>.NotFound("Không tìm thấy phòng học trực tuyến");

            return ApiResponse<VirtualClassroomResponseDto>.Ok(MapToResponseDto(classroom));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy phòng học trực tuyến {Id}", id);
            return ApiResponse<VirtualClassroomResponseDto>.Error("Đã xảy ra lỗi khi lấy thông tin phòng học");
        }
    }

    public async Task<ApiResponse<VirtualClassroomResponseDto>> CreateClassroomAsync(CreateVirtualClassroomDto dto)
    {
        try
        {
            var courseExists = await _context.Courses.AnyAsync(c => c.Id == dto.CourseId);
            if (!courseExists)
                return ApiResponse<VirtualClassroomResponseDto>.NotFound("Không tìm thấy khóa học");

            if (!Enum.TryParse<MeetingPlatform>(dto.Platform, true, out var platform))
                return ApiResponse<VirtualClassroomResponseDto>.BadRequest("Nền tảng không hợp lệ. Chọn: Zoom, GoogleMeet, MSTeams");

            string? meetingUrl = dto.MeetingUrl;
            string? meetingId = dto.MeetingId;
            string? meetingPassword = dto.MeetingPassword;

            var provider = _providerFactory.Resolve(platform);
            if (provider is not null)
            {
                var meetingReq = new CreateMeetingRequest(
                    Topic: dto.Title,
                    Agenda: dto.Description,
                    StartTimeUtc: dto.ScheduledAt.ToUniversalTime(),
                    DurationMinutes: dto.DurationMinutes
                );
                var providerResult = await provider.CreateMeetingAsync(meetingReq);
                if (!providerResult.Success)
                {
                    _logger.LogWarning("Meeting provider {Platform} thất bại: {Error}", platform, providerResult.ErrorMessage);
                    return ApiResponse<VirtualClassroomResponseDto>.Error(
                        $"Không thể tạo cuộc họp trên {platform}: {providerResult.ErrorMessage}");
                }
                meetingUrl = providerResult.JoinUrl;
                meetingId = providerResult.MeetingId;
                meetingPassword = providerResult.Password;
            }

            var classroom = new VirtualClassroom
            {
                Title = dto.Title,
                Description = dto.Description,
                Platform = platform,
                MeetingUrl = meetingUrl,
                MeetingId = meetingId,
                MeetingPassword = meetingPassword,
                ScheduledAt = dto.ScheduledAt,
                DurationMinutes = dto.DurationMinutes,
                CourseId = dto.CourseId,
                Status = ClassroomStatus.Scheduled
            };

            _context.VirtualClassrooms.Add(classroom);
            await _context.SaveChangesAsync();

            await _context.Entry(classroom).Reference(v => v.Course).LoadAsync();

            return ApiResponse<VirtualClassroomResponseDto>.Ok(MapToResponseDto(classroom), "Tạo phòng học trực tuyến thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tạo phòng học trực tuyến");
            return ApiResponse<VirtualClassroomResponseDto>.Error("Đã xảy ra lỗi khi tạo phòng học");
        }
    }

    public async Task<ApiResponse<VirtualClassroomResponseDto>> UpdateClassroomAsync(int id, UpdateVirtualClassroomDto dto)
    {
        try
        {
            var classroom = await _context.VirtualClassrooms
                .Include(v => v.Course)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (classroom is null)
                return ApiResponse<VirtualClassroomResponseDto>.NotFound("Không tìm thấy phòng học trực tuyến");

            if (!Enum.TryParse<MeetingPlatform>(dto.Platform, true, out var platform))
                return ApiResponse<VirtualClassroomResponseDto>.BadRequest("Nền tảng không hợp lệ");

            if (!Enum.TryParse<ClassroomStatus>(dto.Status, true, out var status))
                return ApiResponse<VirtualClassroomResponseDto>.BadRequest("Trạng thái không hợp lệ");

            classroom.Title = dto.Title;
            classroom.Description = dto.Description;
            classroom.Platform = platform;
            classroom.MeetingUrl = dto.MeetingUrl;
            classroom.MeetingId = dto.MeetingId;
            classroom.MeetingPassword = dto.MeetingPassword;
            classroom.ScheduledAt = dto.ScheduledAt;
            classroom.DurationMinutes = dto.DurationMinutes;
            classroom.Status = status;

            await _context.SaveChangesAsync();

            return ApiResponse<VirtualClassroomResponseDto>.Ok(MapToResponseDto(classroom), "Cập nhật phòng học thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi cập nhật phòng học trực tuyến {Id}", id);
            return ApiResponse<VirtualClassroomResponseDto>.Error("Đã xảy ra lỗi khi cập nhật phòng học");
        }
    }

    public async Task<ApiResponse<object?>> DeleteClassroomAsync(int id)
    {
        try
        {
            var classroom = await _context.VirtualClassrooms.FindAsync(id);
            if (classroom is null)
                return ApiResponse<object?>.NotFound("Không tìm thấy phòng học trực tuyến");

            _context.VirtualClassrooms.Remove(classroom);
            await _context.SaveChangesAsync();

            return ApiResponse<object?>.Ok(null, "Xóa phòng học thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xóa phòng học trực tuyến {Id}", id);
            return ApiResponse<object?>.Error("Đã xảy ra lỗi khi xóa phòng học");
        }
    }

    public async Task<ApiResponse<VirtualClassroomResponseDto>> UpdateStatusAsync(int id, string status)
    {
        try
        {
            var classroom = await _context.VirtualClassrooms
                .Include(v => v.Course)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (classroom is null)
                return ApiResponse<VirtualClassroomResponseDto>.NotFound("Không tìm thấy phòng học trực tuyến");

            if (!Enum.TryParse<ClassroomStatus>(status, true, out var classroomStatus))
                return ApiResponse<VirtualClassroomResponseDto>.BadRequest("Trạng thái không hợp lệ. Chọn: Scheduled, Active, Ended, Cancelled");

            classroom.Status = classroomStatus;
            await _context.SaveChangesAsync();

            return ApiResponse<VirtualClassroomResponseDto>.Ok(MapToResponseDto(classroom), "Cập nhật trạng thái thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi cập nhật trạng thái phòng học {Id}", id);
            return ApiResponse<VirtualClassroomResponseDto>.Error("Đã xảy ra lỗi khi cập nhật trạng thái");
        }
    }

    private static VirtualClassroomResponseDto MapToResponseDto(VirtualClassroom v) => new()
    {
        Id = v.Id,
        Title = v.Title,
        Description = v.Description,
        Platform = v.Platform.ToString(),
        MeetingUrl = v.MeetingUrl,
        MeetingId = v.MeetingId,
        MeetingPassword = v.MeetingPassword,
        ScheduledAt = v.ScheduledAt,
        DurationMinutes = v.DurationMinutes,
        Status = v.Status.ToString(),
        CourseId = v.CourseId,
        CourseName = v.Course?.Title ?? string.Empty,
        CreatedAt = v.CreatedAt
    };
}
