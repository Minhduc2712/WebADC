using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ErpOnlineOrder.WebMVC.Services
{
    public static class ErpApiClientHelper
    {
        public static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };

        public static async Task<string?> ReadErrorMessageAsync(HttpResponseMessage response, CancellationToken cancellationToken = default)
        {
            try
            {
                var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions, cancellationToken);

                if (json.TryGetProperty("message", out var msg))
                    return msg.GetString();

                if (json.TryGetProperty("detail", out var detail))
                    return detail.GetString();

                if (json.TryGetProperty("title", out var title) && json.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
                {
                    foreach (var field in errors.EnumerateObject())
                    {
                        if (field.Value.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var item in field.Value.EnumerateArray())
                            {
                                var text = item.GetString();
                                if (!string.IsNullOrWhiteSpace(text))
                                    return text;
                            }
                        }
                    }

                    var titleText = title.GetString();
                    if (!string.IsNullOrWhiteSpace(titleText))
                        return titleText;
                }
            }
            catch { /* ignore */ }
            return response.ReasonPhrase ?? "Lỗi không xác định.";
        }
    }
}
