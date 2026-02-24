using System.Collections.Generic;

namespace ErpOnlineOrder.Application.DTOs.SettingsDTOs
{
    public class SettingIndexViewModel
    {
        public SmtpSettingsDto Smtp { get; set; } = new();
        public IEnumerable<SystemSettingDto> AllSettings { get; set; } = new List<SystemSettingDto>();
    }
}
