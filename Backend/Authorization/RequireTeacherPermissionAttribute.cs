using System.Security.Claims;
using Backend.Common;
using Backend.Models;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Backend.Authorization;

// Đặt cùng [Authorize(Roles = "Admin,Teacher")] trên action/controller. Admin luôn được đi tiếp
// (giữ nguyên hành vi trước đây); Teacher phải có quyền tương ứng trong bảng
// TeacherModulePermissions, kiểm tra trực tiếp từ DB (không nhúng vào JWT) để Admin đổi quyền có
// hiệu lực ngay mà không cần giáo viên đăng nhập lại.
public class RequireTeacherPermissionAttribute : Attribute, IAsyncActionFilter
{
    private readonly PermissionModule _module;
    private readonly PermissionAction _action;

    public RequireTeacherPermissionAttribute(PermissionModule module, PermissionAction action)
    {
        _module = module;
        _action = action;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var user = context.HttpContext.User;

        if (user.IsInRole("Admin"))
        {
            await next();
            return;
        }

        var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
        {
            context.Result = new ObjectResult(ApiResponse<object?>.Forbidden("Bạn không có quyền thực hiện thao tác này"))
            { StatusCode = 403 };
            return;
        }

        var permissionService = context.HttpContext.RequestServices.GetRequiredService<ITeacherPermissionService>();
        var allowed = await permissionService.HasPermissionAsync(userId, _module, _action);
        if (!allowed)
        {
            context.Result = new ObjectResult(ApiResponse<object?>.Forbidden("Bạn không có quyền thực hiện thao tác này"))
            { StatusCode = 403 };
            return;
        }

        await next();
    }
}
