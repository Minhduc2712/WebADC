using ErpOnlineOrder.Application.Constants;
using ErpOnlineOrder.Application.DTOs.SettingsDTOs;
using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Domain.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Application.Services
{
    public class SettingService : ISettingService
    {
        private readonly ISystemSettingRepository _settingRepository;
        private readonly IPermissionService _permissionService;

        public SettingService(ISystemSettingRepository settingRepository, IPermissionService permissionService)
        {
            _settingRepository = settingRepository;
            _permissionService = permissionService;
        }

        public async Task<string?> GetAsync(string key)
        {
            var setting = await _settingRepository.GetByKeyAsync(key);
            return setting?.SettingValue;
        }

        public async Task<IEnumerable<SystemSettingDto>> GetAllAsync(int? userId = null)
        {
            var settings = await _settingRepository.GetAllAsync();
            var list = settings.Select(s => new SystemSettingDto
            {
                Id = s.Id,
                SettingKey = s.SettingKey,
                SettingValue = s.SettingKey == SettingKeys.SmtpPassword ? "****" : s.SettingValue,
                Description = s.Description,
                CreatedBy = s.CreatedBy,
                CreatedAt = s.CreatedAt,
                UpdatedBy = s.UpdatedBy,
                UpdatedAt = s.UpdatedAt
            }).ToList();
            if (userId.HasValue && userId.Value > 0)
            {
                var userPermissions = (await _permissionService.GetUserPermissionsAsync(userId.Value)).ToHashSet();
                foreach (var dto in list)
                    RecordPermissionEnricher.EnrichSetting(dto, userPermissions);
            }
            return list;
        }

        public async Task<bool> SetAsync(string key, string value, int updatedBy)
        {
            var existing = await _settingRepository.GetByKeyAsync(key);
            if (existing != null)
            {
                existing.SettingValue = value;
                existing.UpdatedBy = updatedBy;
                existing.UpdatedAt = System.DateTime.UtcNow;
                await _settingRepository.UpdateAsync(existing);
            }
            else
            {
                var setting = new SystemSetting
                {
                    SettingKey = key,
                    SettingValue = value,
                    CreatedBy = updatedBy,
                    CreatedAt = System.DateTime.UtcNow,
                    UpdatedBy = updatedBy,
                    UpdatedAt = System.DateTime.UtcNow,
                    IsDeleted = false
                };
                await _settingRepository.AddAsync(setting);
            }
            return true;
        }

        public async Task<bool> CreateSettingAsync(string key, string value, string? description, int updatedBy)
        {
            var existing = await _settingRepository.GetByKeyAsync(key);
            if (existing != null) return false;

            var setting = new SystemSetting
            {
                SettingKey = key,
                SettingValue = value ?? "",
                Description = description,
                CreatedBy = updatedBy,
                CreatedAt = DateTime.UtcNow,
                UpdatedBy = updatedBy,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };
            await _settingRepository.AddAsync(setting);
            return true;
        }

        public async Task<bool> UpdateSettingAsync(int id, string value, string? description, int updatedBy)
        {
            var setting = await _settingRepository.GetByIdAsync(id);
            if (setting == null) return false;

            setting.SettingValue = value;
            setting.Description = description;
            setting.UpdatedBy = updatedBy;
            setting.UpdatedAt = DateTime.UtcNow;
            await _settingRepository.UpdateAsync(setting);
            return true;
        }

        public async Task<SmtpSettingsDto> GetSmtpSettingsAsync()
        {
            var host = await GetAsync(SettingKeys.SmtpHost);
            var portStr = await GetAsync(SettingKeys.SmtpPort);
            var useSslStr = await GetAsync(SettingKeys.SmtpUseSsl);
            var fromName = await GetAsync(SettingKeys.SmtpFromName);
            var fromEmail = await GetAsync(SettingKeys.SmtpFromEmail);
            var password = await GetAsync(SettingKeys.SmtpPassword);

            return new SmtpSettingsDto
            {
                Host = host ?? "smtp.gmail.com",
                Port = int.TryParse(portStr, out var p) ? p : 587,
                UseSsl = useSslStr != "false" && useSslStr != "0",
                FromName = fromName ?? "ERP Online Order",
                FromEmail = fromEmail ?? "",
                Password = string.IsNullOrEmpty(password) ? null : password
            };
        }

        public async Task<bool> SaveSmtpSettingsAsync(SmtpSettingsDto dto, int updatedBy)
        {
            await SetAsync(SettingKeys.SmtpHost, dto.Host, updatedBy);
            await SetAsync(SettingKeys.SmtpPort, dto.Port.ToString(), updatedBy);
            await SetAsync(SettingKeys.SmtpUseSsl, dto.UseSsl.ToString().ToLower(), updatedBy);
            await SetAsync(SettingKeys.SmtpFromName, dto.FromName, updatedBy);
            await SetAsync(SettingKeys.SmtpFromEmail, dto.FromEmail, updatedBy);
            if (!string.IsNullOrEmpty(dto.Password) && dto.Password != "****")
            {
                await SetAsync(SettingKeys.SmtpPassword, dto.Password, updatedBy);
            }
            return true;
        }
    }
}
