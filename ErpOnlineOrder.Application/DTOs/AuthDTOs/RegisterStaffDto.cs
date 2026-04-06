using System.ComponentModel.DataAnnotations;

namespace ErpOnlineOrder.Application.DTOs.AuthDTOs
{
    public class RegisterStaffDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Người tạo không hợp lệ.")]
        public int Created_id { get; set; }

        [Required(ErrorMessage = "Tên đăng nhập không được để trống.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Tên đăng nhập phải từ 3 đến 50 ký tự.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email không được để trống.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        [StringLength(200, ErrorMessage = "Email không được vượt quá 200 ký tự.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu không được để trống.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6 đến 100 ký tự.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Họ và tên không được để trống.")]
        [StringLength(100, ErrorMessage = "Họ và tên không được vượt quá 100 ký tự.")]
        public string Full_name { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        [StringLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự.")]
        public string Phone_number { get; set; } = string.Empty;

        [Required(ErrorMessage = "Loại vai trò không được để trống.")]
        [StringLength(50, ErrorMessage = "Loại vai trò không được vượt quá 50 ký tự.")]
        public string Role_type { get; set; } = string.Empty;
    }
}
