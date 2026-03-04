using System.ComponentModel.DataAnnotations;

namespace ErpOnlineOrder.Application.DTOs.AuthDTOs
{
    public class ChangePasswordDto
    {
        public string? Identifier { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu hiện tại")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu hiện tại")]
        public string OldPassword { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu mới từ 6 đến 100 ký tự")]
        [Display(Name = "Mật khẩu mới")]
        public string NewPassword { get; set; } = null!;
    }
}
