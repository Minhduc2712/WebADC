using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ErpOnlineOrder.Application.Interfaces.Services;

namespace ErpOnlineOrder.WebMVC.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequirePermissionAttribute : TypeFilterAttribute
    {
        public RequirePermissionAttribute(params string[] permissions) : base(typeof(RequirePermissionFilter))
        {
            Arguments = new object[] { permissions, false };
        }
        public RequirePermissionAttribute(bool requireAll, params string[] permissions) : base(typeof(RequirePermissionFilter))
        {
            Arguments = new object[] { permissions, requireAll };
        }
    }
    public class RequirePermissionFilter : IAsyncActionFilter
    {
        private readonly string[] _permissions;
        private readonly bool _requireAll;
        private readonly IPermissionService _permissionService;
        private readonly ILogger<RequirePermissionFilter> _logger;

        public RequirePermissionFilter(
            string[] permissions,
            bool requireAll,
            IPermissionService permissionService,
            ILogger<RequirePermissionFilter> logger)
        {
            _permissions = permissions;
            _requireAll = requireAll;
            _permissionService = permissionService;
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Lấy UserId từ Session
            var userId = context.HttpContext.Session.GetInt32("UserId");
            var username = context.HttpContext.Session.GetString("Username");

            // Chưa đăng nhập
            if (userId == null || userId == 0 || string.IsNullOrEmpty(username))
            {
                _logger.LogWarning("Unauthorized access attempt to {Path}", context.HttpContext.Request.Path);

                // Lưu URL hiện tại để redirect sau khi đăng nhập
                var returnUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
                context.HttpContext.Session.SetString("ReturnUrl", returnUrl);

                context.Result = new RedirectToActionResult("Login", "Auth", null);
                return;
            }

            // Kiểm tra ROLE_ADMIN bypass - admin có toàn quyền
            var roles = context.HttpContext.Session.GetString("Roles");
            if (!string.IsNullOrEmpty(roles))
            {
                var roleList = roles.Split(',', StringSplitOptions.RemoveEmptyEntries);
                if (roleList.Contains("ROLE_ADMIN"))
                {
                    // Admin có toàn quyền - tiếp tục xử lý
                    await next();
                    return;
                }
            }

            // Kiểm tra quyền cụ thể cho user không phải admin
            bool hasPermission;

            if (_requireAll)
            {
                // Cần tất cả quyền
                hasPermission = await _permissionService.HasAllPermissionsAsync(userId.Value, _permissions);
            }
            else
            {
                // Chỉ cần 1 trong các quyền
                hasPermission = await _permissionService.HasAnyPermissionAsync(userId.Value, _permissions);
            }

            if (!hasPermission)
            {
                _logger.LogWarning("Từ chối truy cập cho user {Username} (ID: {UserId}) vào {Path}. Quyền yêu cầu: {Permissions}",
                    username, userId, context.HttpContext.Request.Path, string.Join(", ", _permissions));

                // Không có quyền - chuyển đến trang Access Denied
                context.Result = new RedirectToActionResult("AccessDenied", "Home", null);
                return;
            }

            // Có quyền - tiếp tục xử lý
            await next();
        }
    }
}
