using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ErpOnlineOrder.Application.Interfaces.Services;
using System.Security.Claims;

namespace ErpOnlineOrder.WebAPI.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequirePermissionAttribute : TypeFilterAttribute
    {
        public RequirePermissionAttribute(string permissionCode) 
            : base(typeof(RequirePermissionFilter))
        {
            Arguments = new object[] { permissionCode };
        }
    }

    public class RequirePermissionFilter : IAsyncActionFilter
    {
        private readonly string _permissionCode;
        private readonly IPermissionService _permissionService;

        public RequirePermissionFilter(string permissionCode, IPermissionService permissionService)
        {
            _permissionCode = permissionCode;
            _permissionService = permissionService;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Lấy userId từ Claims (JWT token hoặc session)
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

            // Kiểm tra quyền
            var hasPermission = await _permissionService.HasPermissionAsync(userId, _permissionCode);
            if (!hasPermission)
            {
                context.Result = new ObjectResult(new 
                { 
                    success = false, 
                    message = $"Bạn không có quyền thực hiện hành động này. Yêu cầu quyền: {_permissionCode}" 
                })
                {
                    StatusCode = 403 // Forbidden
                };
                return;
            }

            await next();
        }
    }
}
