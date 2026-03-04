using System.Net.Http.Json;
using System.Text.Json;
using ErpOnlineOrder.Application.DTOs.AuthDTOs;

namespace ErpOnlineOrder.WebMVC.Services
{
    public class AuthApiClient : IAuthApiClient
    {
        private readonly HttpClient _httpClient;
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public AuthApiClient(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("ErpApi");
        }

        public async Task<LoginResponseDto?> LoginAsync(LoginUserDto dto, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.PostAsJsonAsync("auth/login", dto, JsonOptions, cancellationToken);
            if (!response.IsSuccessStatusCode)
                return null;

            var wrapper = await response.Content.ReadFromJsonAsync<LoginApiResponse>(JsonOptions, cancellationToken);
            return wrapper?.Success == true ? wrapper.User : null;
        }

        public async Task<(bool Success, string? ErrorMessage)> RegisterCustomerAsync(RegisterCustomerDto dto, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.PostAsJsonAsync("auth/register-customer", dto, JsonOptions, cancellationToken);
            if (!response.IsSuccessStatusCode)
                return (false, await ReadErrorMessageAsync(response, cancellationToken));

            var body = await response.Content.ReadFromJsonAsync<SuccessApiResponse>(JsonOptions, cancellationToken);
            return body?.Success == true ? (true, null) : (false, "Đăng ký thất bại.");
        }

        public async Task<(bool Success, string? ErrorMessage)> RegisterStaffAsync(RegisterStaffDto dto, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.PostAsJsonAsync("auth/register-staff", dto, JsonOptions, cancellationToken);
            if (!response.IsSuccessStatusCode)
                return (false, await ReadErrorMessageAsync(response, cancellationToken));

            var body = await response.Content.ReadFromJsonAsync<SuccessApiResponse>(JsonOptions, cancellationToken);
            return body?.Success == true ? (true, null) : (false, "Tạo tài khoản thất bại.");
        }

        public async Task<(bool Success, string? ErrorMessage)> ChangePasswordAsync(ChangePasswordDto dto, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.PostAsJsonAsync("auth/change-password", dto, JsonOptions, cancellationToken);
            if (!response.IsSuccessStatusCode)
                return (false, await ReadErrorMessageAsync(response, cancellationToken));

            var body = await response.Content.ReadFromJsonAsync<SuccessApiResponse>(JsonOptions, cancellationToken);
            return body?.Success == true ? (true, null) : (false, "Đổi mật khẩu thất bại.");
        }

        private static async Task<string?> ReadErrorMessageAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            try
            {
                var json = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
                if (json.TryGetProperty("message", out var msg))
                    return msg.GetString();
            }
            catch { /* ignore */ }
            return response.ReasonPhrase ?? "Lỗi không xác định.";
        }

        private class LoginApiResponse
        {
            public bool Success { get; set; }
            public LoginResponseDto? User { get; set; }
        }

        private class SuccessApiResponse
        {
            public bool Success { get; set; }
        }
    }
}
