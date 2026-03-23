using System.ComponentModel.DataAnnotations;

namespace ErpOnlineOrder.Application.DTOs.AuthDTOs
{
    public class ForgotPasswordDto
    {
        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Base URL của WebMVC để tạo link đặt lại mật khẩu, vd: https://localhost:7001
        /// </summary>
        [Required]
        public string ResetBaseUrl { get; set; } = string.Empty;
    }
}
