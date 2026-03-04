using System.ComponentModel.DataAnnotations;

namespace ErpOnlineOrder.Application.DTOs.AuthDTOs
{
    public class RegisterCustomerDto
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        [Display(Name = "Tên đăng nhập")]
        [StringLength(100, MinimumLength = 3)]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu từ 6 đến 100 ký tự")]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = null!;
    }
}
