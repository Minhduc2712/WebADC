using System.Net.Http.Json;
using System.Text.Json;
using ErpOnlineOrder.Application.DTOs.AuthDTOs;
using ErpOnlineOrder.WebMVC.Services.Interfaces;

namespace ErpOnlineOrder.WebMVC.Services
{
    public class AuthApiClient : BaseApiClient, IAuthApiClient
    {
        public AuthApiClient(IHttpClientFactory factory) : base(factory.CreateClient("ErpApi"))
        {
        }

        public async Task<LoginResponseDto?> LoginAsync(LoginUserDto dto, CancellationToken cancellationToken = default)
        {
            var (data, _) = await PostAsync<LoginUserDto, LoginResponseDto>("auth/login", dto, cancellationToken);
            return data;
        }

        public async Task<(bool Success, string? ErrorMessage)> RegisterCustomerAsync(RegisterCustomerDto dto, CancellationToken cancellationToken = default)
        {
            return await PostWithoutReturnAsync("auth/register-customer", dto, cancellationToken);
        }

        public async Task<(bool Success, string? ErrorMessage)> FinalizeCustomerRegistrationAsync(FinalizeCustomerRegistrationDto dto, CancellationToken cancellationToken = default)
        {
            return await PostWithoutReturnAsync("auth/register-customer-finalize", dto, cancellationToken);
        }

        public async Task<(bool Success, string? ErrorMessage)> RegisterStaffAsync(RegisterStaffDto dto, CancellationToken cancellationToken = default)
        {
            return await PostWithoutReturnAsync("auth/register-staff", dto, cancellationToken);
        }

        public async Task<(bool Success, string? ErrorMessage)> ChangePasswordAsync(ChangePasswordDto dto, CancellationToken cancellationToken = default)
        {
            return await PostWithoutReturnAsync("auth/change-password", dto, cancellationToken);
        }

        public async Task<string?> GenerateTokenAsync(string username, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.PostAsJsonAsync("auth/generate-token", new { Username = username }, _jsonOptions, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var json = JsonDocument.Parse(content);
                if (json.RootElement.TryGetProperty("success", out var success) && success.GetBoolean() && json.RootElement.TryGetProperty("token", out var token))
                {
                    return token.GetString();
                }
            }
            return null;
        }

        public async Task<LoginResponseDto?> AutoLoginAsync(string username, string token, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.PostAsJsonAsync("auth/auto-login", new { Username = username, Token = token }, _jsonOptions, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var json = JsonDocument.Parse(content);
                if (json.RootElement.TryGetProperty("success", out var success) && success.GetBoolean() && json.RootElement.TryGetProperty("user", out var userEl))
                {
                    return JsonSerializer.Deserialize<LoginResponseDto>(userEl.GetRawText(), _jsonOptions);
                }
            }
            return null;
        }

        public async Task<bool> CheckEmailExistsAsync(string email, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.GetAsync($"auth/check-email?email={Uri.EscapeDataString(email)}", cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var json = JsonDocument.Parse(content);
                if (json.RootElement.TryGetProperty("exists", out var exists))
                    return exists.GetBoolean();
            }
            return false;
        }
    }
}
