using System.Net.Http.Headers;

namespace ErpOnlineOrder.WebMVC.Services
{
    /// <summary>
    /// Gửi UserId từ Session (MVC) lên API qua header để API có thể kiểm tra quyền.
    /// </summary>
    public class ErpApiUserIdHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ErpApiUserIdHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var ctx = _httpContextAccessor.HttpContext;
            var userId = ctx?.Session?.GetInt32("UserId");
            if (userId.HasValue)
                request.Headers.TryAddWithoutValidation("X-User-Id", userId.Value.ToString());

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
