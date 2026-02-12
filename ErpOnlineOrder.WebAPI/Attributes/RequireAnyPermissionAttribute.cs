using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ErpOnlineOrder.Application.Interfaces.Services;
using System.Security.Claims;

namespace ErpOnlineOrder.WebAPI.Attributes
{
    /// <summary>
    /// Cho phép truy cập nếu user có BẤT KỲ quyền nào trong danh sách.
    /// Dùng cho các endpoint cần nhiều quyền thay thế (vd: ProductView hoặc OrderCreate).
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequireAnyPermissionAttribute : TypeFilterAttribute
    {
        public RequireAnyPermissionAttribute(params string[] permissionCodes)
            : base(typeof(RequireAnyPermissionFilter))
        {
            Arguments = new object[] { permissionCodes };
        }
    }

    public class RequireAnyPermissionFilter : IAsyncActionFilter
    {
        private readonly string[] _permissionCodes;
        private readonly IPermissionService _permissionService;

        public RequireAnyPermissionFilter(string[] permissionCodes, IPermissionService permissionService)
        {
            _permissionCodes = permissionCodes ?? Array.Empty<string>();
            _permissionService = permissionService;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var userIdClaim = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)
                ?? context.HttpContext.User.FindFirst("UserId");

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                context.Result = new UnauthorizedObjectResult(new
                {
                    success = false,
                    message = "Bạn chưa đăng nhập"
                });
                return;
            }

            if (_permissionCodes.Length == 0)
            {
                await next();
                return;
            }

            var hasPermission = await _permissionService.HasAnyPermissionAsync(userId, _permissionCodes);
            if (!hasPermission)
            {
                context.Result = new ObjectResult(new
                {
                    success = false,
                    message = $"Bạn không có quyền thực hiện hành động này. Yêu cầu một trong các quyền: {string.Join(", ", _permissionCodes)}"
                })
                {
                    StatusCode = 403
                };
                return;
            }

            await next();
        }
    }
}
