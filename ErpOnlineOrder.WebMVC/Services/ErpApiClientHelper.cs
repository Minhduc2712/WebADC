using System.Net.Http.Json;
using System.Text.Json;

namespace ErpOnlineOrder.WebMVC.Services
{
    public static class ErpApiClientHelper
    {
        public static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public static async Task<string?> ReadErrorMessageAsync(HttpResponseMessage response, CancellationToken cancellationToken = default)
        {
            try
            {
                var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions, cancellationToken);
                if (json.TryGetProperty("message", out var msg))
                    return msg.GetString();
            }
            catch { /* ignore */ }
            return response.ReasonPhrase ?? "Lỗi không xác định.";
        }
    }
}
