using ErpOnlineOrder.Application.DTOs.SettingsDTOs;

namespace ErpOnlineOrder.WebMVC.Services
{
    public interface ISettingApiClient
    {
        Task<SmtpSettingsDto?> GetSmtpSettingsAsync(CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error)> SaveSmtpSettingsAsync(SmtpSettingsDto dto, CancellationToken cancellationToken = default);
        Task<IEnumerable<SystemSettingDto>?> GetAllSettingsAsync(CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error)> CreateSettingAsync(CreateSettingDto dto, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error)> UpdateSettingAsync(int id, UpdateSettingDto dto, CancellationToken cancellationToken = default);
    }
}
