namespace ErpOnlineOrder.Application.DTOs.SettingsDTOs
{
    public class SmtpSettingsDto
    {
        public string Host { get; set; } = "smtp.gmail.com";
        public int Port { get; set; } = 587;
        public bool UseSsl { get; set; } = true;
        public string FromName { get; set; } = "ERP Online Order";
        public string FromEmail { get; set; } = "";
        public string? Password { get; set; }
    }
}
