using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Application.Interfaces.Services;

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
            IUserRepository userRepository,
            IPermissionService permissionService,
            IRememberMeService rememberMeService)
        {
            // B? qua các du?ng d?n không c?n xác th?c
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
                var restored = await TryRestoreSessionFromCookie(context, userRepository, permissionService, rememberMeService);

                if (!restored)
                {
                }
            }
            else
            {
                var isValid = await ValidateSession(context, userRepository, permissionService, userId.Value);
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
            IUserRepository userRepository,
            IPermissionService permissionService,
            IRememberMeService rememberMeService)
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

                if (!await rememberMeService.ValidateTokenAsync(cookieUsername, cookieToken))
                {
                    ClearRememberMeCookies(context);
                    return false;
                }

                var user = await userRepository.GetByUsernameAsync(cookieUsername);

                if (user == null || !user.Is_active || user.Is_deleted)
                {
                    ClearRememberMeCookies(context);
                    return false;
                }

                await SetUserSession(context, user, permissionService);

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
        private async Task<bool> ValidateSession(HttpContext context, IUserRepository userRepository, IPermissionService permissionService, int userId)
        {
            try
            {
                // Kiểm tra định kỳ (mỗi 5 phút)
                var lastCheck = context.Session.GetString("LastValidationCheck");
                if (!string.IsNullOrEmpty(lastCheck))
                {
                    var lastCheckTime = DateTime.Parse(lastCheck);
                    if ((DateTime.Now - lastCheckTime).TotalMinutes < 5)
                    {
                        return true; // Chưa đến lúc kiểm tra
                    }
                }

                var user = await userRepository.GetByIdAsync(userId);
                if (user == null || !user.Is_active || user.Is_deleted)
                {
                    return false;
                }

                // Đồng bộ quyền từ DB vào Session để menu và controller luôn khớp (tránh 403 khi admin đã đổi quyền)
                var permissions = await permissionService.GetUserPermissionsAsync(userId);
                context.Session.SetString("Permissions", string.Join(",", permissions));

                // Cập nhật thời gian kiểm tra
                context.Session.SetString("LastValidationCheck", DateTime.Now.ToString("o"));
                return true;
            }
            catch
            {
                return false;
            }
        }
        private async Task SetUserSession(
            HttpContext context,
            Domain.Models.User user,
            IPermissionService permissionService)
        {
            context.Session.SetInt32("UserId", user.Id);
            context.Session.SetString("Username", user.Username);
            context.Session.SetString("Email", user.Email);

            // Luu tên d?y d?
            var fullName = user.Staff?.Full_name ?? user.Customer?.Full_name ?? user.Username;
            context.Session.SetString("FullName", fullName);

            // Luu roles
            var roles = user.User_roles?
                .Where(ur => !ur.Is_deleted && ur.Role != null && !ur.Role.Is_deleted)
                .Select(ur => ur.Role!.Role_name)
                .ToList() ?? new List<string>();
            context.Session.SetString("Roles", string.Join(",", roles));

            // Luu permissions
            var permissions = await permissionService.GetUserPermissionsAsync(user.Id);
            context.Session.SetString("Permissions", string.Join(",", permissions));

            // Luu lo?i user
            if (user.Staff != null)
            {
                context.Session.SetString("UserType", "Staff");
                context.Session.SetString("StaffCode", user.Staff.Staff_code ?? "");
            }
            else if (user.Customer != null)
            {
                context.Session.SetString("UserType", "Customer");
                context.Session.SetString("CustomerCode", user.Customer.Customer_code ?? "");
            }

            // Luu th?i gian dang nh?p
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
