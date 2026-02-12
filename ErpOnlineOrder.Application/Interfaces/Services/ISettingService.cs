using ErpOnlineOrder.Application.DTOs.SettingsDTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Application.Interfaces.Services
{
    public interface ISettingService
    {
        Task<string?> GetAsync(string key);
        Task<IEnumerable<SystemSettingDto>> GetAllAsync();
        Task<bool> SetAsync(string key, string value, int updatedBy);
        Task<SmtpSettingsDto> GetSmtpSettingsAsync();
        Task<bool> SaveSmtpSettingsAsync(SmtpSettingsDto dto, int updatedBy);
    }
}
