using System.Net.Http.Json;
using ErpOnlineOrder.Application.DTOs.SettingsDTOs;
using ErpOnlineOrder.WebMVC.Services.Interfaces;

namespace ErpOnlineOrder.WebMVC.Services
{
    public class SettingApiClient : BaseApiClient, ISettingApiClient
    {
        public SettingApiClient(IHttpClientFactory factory) : base(factory.CreateClient("ErpApi"))
        {
        }

        public async Task<SmtpSettingsDto?> GetSmtpSettingsAsync(CancellationToken cancellationToken = default)
        {
            return await GetAsync<SmtpSettingsDto>("setting/smtp", cancellationToken);
        }

        public async Task<(bool Success, string? Error)> SaveSmtpSettingsAsync(SmtpSettingsDto dto, CancellationToken cancellationToken = default)
        {
            return await PutAsync("setting/smtp", dto, cancellationToken);
        }

        public async Task<IEnumerable<SystemSettingDto>?> GetAllSettingsAsync(CancellationToken cancellationToken = default)
        {
            return await GetAsync<IEnumerable<SystemSettingDto>>("setting", cancellationToken);
        }

        public async Task<(bool Success, string? Error)> CreateSettingAsync(CreateSettingDto dto, CancellationToken cancellationToken = default)
        {
            return await PostWithoutReturnAsync("setting", dto, cancellationToken);
        }

        public async Task<(bool Success, string? Error)> UpdateSettingAsync(int id, UpdateSettingDto dto, CancellationToken cancellationToken = default)
        {
            return await PutAsync($"setting/{id}", dto, cancellationToken);
        }
    }
}
