using ErpOnlineOrder.WebMVC.Services.Interfaces;
using ErpOnlineOrder.Application.DTOs.AuthDTOs;
using System.Threading.Tasks;

namespace ErpOnlineOrder.WebMVC.Middleware
{
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuthenticationMiddleware> _logger;

        public AuthenticationMiddleware(RequestDelegate next, ILogger<AuthenticationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(
            HttpContext context,
            IAuthApiClient authApiClient,
            IPermissionApiClient permissionApiClient)
        {
            // Bỏ qua các đường dẫn không cần xác thực
            var path = context.Request.Path.Value?.ToLower() ?? "";
            if (IsPublicPath(path))
            {
                await _next(context);
                return;
            }

            var userId = context.Session.GetInt32("UserId");
            var username = context.Session.GetString("Username");

            if (userId == null || string.IsNullOrEmpty(username))
            {
                await TryRestoreSessionFromCookie(context, authApiClient);
            }
            else
            {
                var isValid = await ValidateSession(context, permissionApiClient, userId.Value);
                if (!isValid)
                {
                    ClearSession(context);
                }
            }

            await _next(context);
        }
        private bool IsPublicPath(string path)
        {
            var publicPaths = new[]
            {
                "/auth/login",
                "/auth/customerregister",
                "/auth/forgotpassword",
                "/auth/resetpassword",
                "/home/error",
                "/css",
                "/js",
                "/lib",
                "/images",
                "/favicon.ico"
            };

            return publicPaths.Any(p => path.StartsWith(p));
        }
        private async Task<bool> TryRestoreSessionFromCookie(
            HttpContext context,
            IAuthApiClient authApiClient)
        {
            try
            {
                if (!context.Request.Cookies.ContainsKey("RememberMe_Username") ||
                    !context.Request.Cookies.ContainsKey("RememberMe_Token"))
                {
                    return false;
                }

                var cookieUsername = context.Request.Cookies["RememberMe_Username"];
                var cookieToken = context.Request.Cookies["RememberMe_Token"];

                if (string.IsNullOrEmpty(cookieUsername) || string.IsNullOrEmpty(cookieToken))
                {
                    return false;
                }

                var response = await authApiClient.AutoLoginAsync(cookieUsername, cookieToken);
                if (response == null)
                {
                    ClearRememberMeCookies(context);
                    return false;
                }

                SetUserSessionFromApiResult(context, response);

                _logger.LogInformation("Session restored from cookie for user: {Username}", cookieUsername);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring session from cookie");
                ClearRememberMeCookies(context);
                return false;
            }
        }
        private async Task<bool> ValidateSession(HttpContext context, IPermissionApiClient permissionApiClient, int userId)
        {
            try
            {
                // Kiểm tra định kỳ (mỗi 5 phút)
                var lastCheck = context.Session.GetString("LastValidationCheck");
                if (!string.IsNullOrEmpty(lastCheck))
                {
                    if (DateTime.TryParse(lastCheck, out var lastCheckTime))
                    {
                        if ((DateTime.Now - lastCheckTime).TotalMinutes < 5)
                        {
                            return true; // Chưa đến lúc kiểm tra
                        }
                    }
                }

                // Đồng bộ quyền từ DB vào Session để menu và controller luôn khớp (tránh 403 khi admin đã đổi quyền)
                var permissions = await permissionApiClient.GetUserPermissionCodesAsync(userId);
                if (permissions != null)
                {
                    context.Session.SetString("Permissions", string.Join(",", permissions));
                    context.Session.SetString("LastValidationCheck", DateTime.Now.ToString("o"));
                }
                return true;
            }
            catch
            {
                // Bỏ qua lỗi để tránh bắt người dùng đăng nhập lại khi mạng chập chờn
                return true;
            }
        }
        private void SetUserSessionFromApiResult(HttpContext context, LoginResponseDto dto)
        {
            context.Session.SetInt32("UserId", dto.UserId);
            context.Session.SetString("Username", dto.Username);
            context.Session.SetString("Email", dto.Email ?? "");
            context.Session.SetString("FullName", dto.FullName ?? dto.Username);
            context.Session.SetString("Roles", string.Join(",", dto.Roles ?? new List<string>()));
            context.Session.SetString("Permissions", string.Join(",", dto.Permissions ?? new List<string>()));
            context.Session.SetString("UserType", dto.UserType ?? "");
            context.Session.SetString("StaffCode", dto.StaffCode ?? "");
            context.Session.SetString("CustomerCode", dto.CustomerCode ?? "");
            context.Session.SetString("LoginTime", DateTime.Now.ToString("o"));
            context.Session.SetString("LastValidationCheck", DateTime.Now.ToString("o"));
        }
        private void ClearSession(HttpContext context)
        {
            context.Session.Clear();
        }
        private void ClearRememberMeCookies(HttpContext context)
        {
            context.Response.Cookies.Delete("RememberMe_Username");
            context.Response.Cookies.Delete("RememberMe_Token");
        }
    }
    public static class AuthenticationMiddlewareExtensions
    {
        public static IApplicationBuilder UseSessionAuthentication(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuthenticationMiddleware>();
        }
    }
}
