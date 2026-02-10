using System.Security.Claims;

namespace ErpOnlineOrder.WebAPI.Middleware
{
    /// <summary>
    /// Đọc header X-User-Id (do WebMVC gửi khi gọi API) và gán vào HttpContext.User để RequirePermission hoạt động.
    /// </summary>
    public class UserIdFromHeaderMiddleware
    {
        private readonly RequestDelegate _next;
        private const string UserIdHeader = "X-User-Id";

        public UserIdFromHeaderMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue(UserIdHeader, out var value) &&
                int.TryParse(value, out int userId) && userId > 0)
            {
                var identity = new ClaimsIdentity("Custom");
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userId.ToString()));
                identity.AddClaim(new Claim("UserId", userId.ToString()));
                context.User = new ClaimsPrincipal(identity);
            }

            await _next(context);
        }
    }
}
