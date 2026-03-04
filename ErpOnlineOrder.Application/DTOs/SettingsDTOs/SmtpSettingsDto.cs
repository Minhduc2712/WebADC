using System.ComponentModel.DataAnnotations;

namespace ErpOnlineOrder.Application.DTOs.SettingsDTOs
{
    public class SmtpSettingsDto
    {
        [Required(ErrorMessage = "Nhập máy chủ SMTP")]
        [Display(Name = "Máy chủ SMTP")]
        public string Host { get; set; } = "smtp.gmail.com";

        [Range(1, 65535, ErrorMessage = "Cổng phải từ 1-65535")]
        public int Port { get; set; } = 587;

        public bool UseSsl { get; set; } = true;

        [Display(Name = "Tên hiển thị")]
        public string FromName { get; set; } = "ERP Online Order";

        [Required(ErrorMessage = "Nhập email gửi")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email gửi")]
        public string FromEmail { get; set; } = "";

        [Display(Name = "Mật khẩu")]
        public string? Password { get; set; }
    }
}
