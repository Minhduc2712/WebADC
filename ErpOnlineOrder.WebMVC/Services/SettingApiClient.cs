using System.Net.Http.Json;
using ErpOnlineOrder.Application.DTOs.SettingsDTOs;

namespace ErpOnlineOrder.WebMVC.Services
{
    public class SettingApiClient : ISettingApiClient
    {
        private readonly HttpClient _http;

        public SettingApiClient(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("ErpApi");
        }

        public async Task<SmtpSettingsDto?> GetSmtpSettingsAsync(CancellationToken cancellationToken = default)
        {
            var response = await _http.GetAsync("setting/smtp", cancellationToken);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<SmtpSettingsDto>(ErpApiClientHelper.JsonOptions, cancellationToken);
        }

        public async Task<(bool Success, string? Error)> SaveSmtpSettingsAsync(SmtpSettingsDto dto, CancellationToken cancellationToken = default)
        {
            var response = await _http.PutAsJsonAsync("setting/smtp", dto, ErpApiClientHelper.JsonOptions, cancellationToken);
            if (response.IsSuccessStatusCode) return (true, null);
            return (false, await ErpApiClientHelper.ReadErrorMessageAsync(response, cancellationToken));
        }

        public async Task<IEnumerable<SystemSettingDto>?> GetAllSettingsAsync(CancellationToken cancellationToken = default)
        {
            var response = await _http.GetAsync("setting", cancellationToken);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<IEnumerable<SystemSettingDto>>(ErpApiClientHelper.JsonOptions, cancellationToken);
        }

        public async Task<(bool Success, string? Error)> CreateSettingAsync(CreateSettingDto dto, CancellationToken cancellationToken = default)
        {
            var response = await _http.PostAsJsonAsync("setting", dto, ErpApiClientHelper.JsonOptions, cancellationToken);
            if (response.IsSuccessStatusCode) return (true, null);
            return (false, await ErpApiClientHelper.ReadErrorMessageAsync(response, cancellationToken));
        }

        public async Task<(bool Success, string? Error)> UpdateSettingAsync(int id, UpdateSettingDto dto, CancellationToken cancellationToken = default)
        {
            var response = await _http.PutAsJsonAsync($"setting/{id}", dto, ErpApiClientHelper.JsonOptions, cancellationToken);
            if (response.IsSuccessStatusCode) return (true, null);
            return (false, await ErpApiClientHelper.ReadErrorMessageAsync(response, cancellationToken));
        }
    }
}
